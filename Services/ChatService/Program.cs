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
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddOpenApi();

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
builder.Services.AddSingleton<RabbitMqConnectionFactory>();
builder.Services.AddScoped<IChatConsumer, ChatConsumer>();

// Add JWT Authentication
builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = "Bearer";
        options.DefaultChallengeScheme = "Bearer";
    })
    .AddJwtBearer("Bearer", options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = "http://localhost:5299",
            ValidateAudience = false,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("fmcE8xVOt3k0CgmfCyuVEkFAZxnQbql5")), // For testing, the key will be removed later
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
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins("http://localhost:5173", "http://localhost:5057", "http://localhost:3000")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseCors();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHub<ChatHub>("/hubs/chat");

app.UseHttpsRedirection();
app.Run();