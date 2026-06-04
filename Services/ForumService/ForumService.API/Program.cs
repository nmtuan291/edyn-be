using System.Text;
using ForumService.ForumService.Application.Interfaces.UnitOfWork;
using ForumService.ForumService.Application.Permissions;
using ForumService.ForumService.Infrastructure.UnitOfWork;
using ForumService.ForumService.API.Middlewares;
using ForumService.ForumService.Application;
using ForumService.ForumService.Application.Interfaces.Repositories;
using ForumService.ForumService.Application.Interfaces.Services;
using Microsoft.EntityFrameworkCore;
using ForumService.ForumService.Infrastructure.Data;
using ForumService.ForumService.Infrastructure.Repositories;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;
using StackExchange.Redis;
using UserService.Grpc;
using Edyn.Telemetry;
using OpenTelemetry.Trace;
using RabbitMQ.Client;
using System.Threading.Channels;
using ForumService.ForumService.Infrastructure.Messaging;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddEdynTelemetry(builder.Configuration, "edyn-forum-service", tracing =>
{
    tracing.AddEntityFrameworkCoreInstrumentation();
    tracing.AddGrpcClientInstrumentation();
    tracing.AddRedisInstrumentation();
});
    
// Add DbContext
builder.Services.AddDbContext<ForumDbContext>(options => options.UseNpgsql(builder.Configuration.GetConnectionString("WebApiDatabase")));


// Add dependency
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IForumThreadService, ForumThreadService>();
builder.Services.AddScoped<IForumService, ForumService.ForumService.Application.ForumService>();
builder.Services.AddScoped<IThreadRepository, ThreadRepository>();
builder.Services.AddScoped<IForumRepository, ForumRepository>();
builder.Services.AddScoped<ICommentNotificationSender, CommentNotificationSender>();
builder.Services.AddScoped<IHomeFeedService, HomeFeedService>();
builder.Services.AddForumRolePermissionStrategies();
builder.Services.AddScoped<IPermissionService, PermissionService>();

// Messaging / RabbitMQ
builder.Services.AddSingleton<IConnectionFactory>(sp =>
{
    var config = sp.GetRequiredService<IConfiguration>();
    var hostName = config["RabbitMQ:HostName"] ?? "localhost";
    var factory = new ConnectionFactory
    {
        HostName = hostName,
        Port = config.GetValue<int>("RabbitMQ:Port", 5672),
        UserName = config["RabbitMQ:UserName"] ?? "guest",
        Password = config["RabbitMQ:Password"] ?? "guest",
        VirtualHost = config["RabbitMQ:VirtualHost"] ?? "/"
    };
    if (config.GetValue("RabbitMQ:UseTls", false))
    {
        factory.Ssl = new SslOption { Enabled = true, ServerName = hostName };
    }
    return factory;
});
builder.Services.AddSingleton<RabbitMqConnectionFactory>();
builder.Services.AddSingleton(new BoundedChannelOptions(1000) { FullMode = BoundedChannelFullMode.DropWrite });
builder.Services.AddSingleton<BoundedChannelBuffer<TelemetryLog>, TelemetryBuffer>();
builder.Services.AddHostedService<TelemetryPublisherService>();

builder.Services.AddAuthentication("Bearer")
    .AddJwtBearer("Bearer", options =>
    {
        options.Authority = builder.Configuration["Jwt:Authority"];
        options.RequireHttpsMetadata = builder.Configuration.GetValue("Jwt:RequireHttpsMetadata", !builder.Environment.IsDevelopment());
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateAudience = false,
            ValidateIssuer = false
        };
    });

// AutoMapper
builder.Services.AddAutoMapper(cfg => {}, typeof(Program));

// Redis
builder.Services.AddSingleton<IConnectionMultiplexer>(sp => 
    ConnectionMultiplexer.Connect(builder.Configuration["Redis:ConnectionString"]));

builder.Services.AddGrpcClient<UserProfileService.UserProfileServiceClient>(option =>
{
    option.Address = new Uri(builder.Configuration["Grpc:UserServiceUrl"]!);
})
.ConfigurePrimaryHttpMessageHandler(sp =>
{
    var handler = new HttpClientHandler();
    if (builder.Environment.IsDevelopment())
    {
        handler.ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;
    }
    return handler;
});

var app = builder.Build();

// Apply pending database migrations
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ForumDbContext>();
    db.Database.Migrate();
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

// Add middleware
app.UseMiddleware<ErrorHandlingMiddleware>();

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
