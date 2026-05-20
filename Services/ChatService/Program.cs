using ChatService.ChatService.Infrastructure.Data;
using ChatService.ChatService.Infrastructure.Repositories;
using ChatService.ChatService.Application.Interfaces.Repositories;
using ChatService.ChatService.Application.Interfaces.Services;
using ChatService.ChatService.Application.Services;
using ChatService.ChatService.Application.Interfaces.Messaging;
using ChatService.ChatService.Infrastructure.Messaging;
using ChatService.ChatService.API.Hubs;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;
using Edyn.Telemetry;
using OpenTelemetry.Trace;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddEdynTelemetry(builder.Configuration, "edyn-chat-service", tracing =>
{
    tracing.AddEntityFrameworkCoreInstrumentation();
});

// Add SignalR
builder.Services.AddSignalR();

// Add DbContext
builder.Services.AddDbContext<ChatDbContext>(options => 
    options.UseNpgsql(builder.Configuration.GetConnectionString("WebApiDatabase")));

// Register Repositories
builder.Services.AddScoped<IChatRepository, ChatRepository>();

// Register Services
builder.Services.AddScoped<IChatManagementService, ChatManagementService>();
builder.Services.AddSingleton<RabbitMqConnectionFactory>();
builder.Services.AddScoped<IChatPublisher, ChatPublisher>();

// Register RabbitMQ dependencies
builder.Services.AddSingleton<IChatConsumer, ChatConsumer>();

// Add JWT Authentication
builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = "Bearer";
        options.DefaultChallengeScheme = "Bearer";
    })
    .AddJwtBearer("Bearer", options =>
    {
        // Auth issues RS256 tokens; use OIDC metadata + JWKS from Auth (same as ApiGateway / ForumService).
        options.Authority = builder.Configuration["Jwt:Authority"];
        options.RequireHttpsMetadata = builder.Configuration.GetValue("Jwt:RequireHttpsMetadata", !builder.Environment.IsDevelopment());
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateAudience = true,
            ValidAudience = builder.Configuration["Jwt:Audience"],
        };

        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var accessToken = context.Request.Query["access_token"];
                var path = context.HttpContext.Request.Path;
                if (!string.IsNullOrEmpty(accessToken) &&
                    path.StartsWithSegments("/hubs/chat"))
                {
                    context.Token = accessToken;
                }
                return Task.CompletedTask;
            }
        };
    });

// Add CORS
var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins(allowedOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

var app = builder.Build();

// Apply pending database migrations
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ChatDbContext>();
    db.Database.Migrate();
}

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseCors();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHub<ChatHub>("/hubs/chat");
app.Run();