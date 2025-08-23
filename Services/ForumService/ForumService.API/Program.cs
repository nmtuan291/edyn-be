using System.Text;
using ForumService.ForumService.Application.Interfaces.UnitOfWork;
using ForumService.ForumService.Infrastructure.UnitOfWork;
using ForumService.ForumService.API.Middlewares;
using ForumService.ForumService.Application;
using ForumService.ForumService.Application.Interfaces.Services;
using Microsoft.EntityFrameworkCore;
using ForumService.ForumService.Infrastructure.Data;
using ForumService.ForumService.Infrastructure.Interfaces;
using ForumService.ForumService.Infrastructure.Repositories;
using Microsoft.IdentityModel.Tokens;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
    
// Add DbContext
builder.Services.AddDbContext<ForumDbContext>(options => options.UseNpgsql(builder.Configuration.GetConnectionString("WebApiDatabase")));


// Add dependency
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IForumThreadService, ForumThreadService>();
builder.Services.AddScoped<IForumService, ForumService.ForumService.Application.ForumService>();
builder.Services.AddScoped<IThreadRepository, ThreadRepository>();
builder.Services.AddScoped<IForumRepository, ForumRepository>();
builder.Services.AddScoped<ICommentNotificationSender, CommentNotificationSender>();

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
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Secret_key"]))
        };
    });

// AutoMapper
builder.Services.AddAutoMapper(cfg => {}, typeof(Program));

// Redis
builder.Services.AddSingleton<IConnectionMultiplexer>(sp => 
    ConnectionMultiplexer.Connect(builder.Configuration["Redis:ConnectionString"]));


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// Add middleware
app.UseMiddleware<ErrorHandlingMiddleware>();

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
