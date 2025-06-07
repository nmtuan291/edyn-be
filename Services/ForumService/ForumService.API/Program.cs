using ForumService.ForumService.Application.Interfaces.UnitOfWork;
using ForumService.ForumService.Infrastructure.UnitOfWork;
using ForumService.ForumService.API.Middlewares;
using ForumService.ForumService.Application;
using ForumService.ForumService.Application.Interfaces.Services;
using Microsoft.EntityFrameworkCore;
using ForumService.ForumService.Infrastructure.Data;

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

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// Add middleware
app.UseMiddleware<ErrorHandlingMiddleware>();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
