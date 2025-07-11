using Microsoft.EntityFrameworkCore;
using UserService.UserService.API.Grpc;
using UserService.UserService.Application;
using UserService.UserService.Application.Interfaces.Repositories;
using UserService.UserService.Application.Interfaces.Services;
using UserService.UserService.Domain.Entities;
using UserService.UserService.Infrastructure.Data;
using UserService.UserService.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();


builder.Services.AddGrpc();

builder.Services.AddDbContext<UserDbContext>(options => options.UseNpgsql(builder.Configuration.GetConnectionString("WebApiDatabase")));

builder.Services.AddScoped<IUserProfileService, UserProfileService>();
builder.Services.AddScoped<IUserProfileRepository, UserProfileRepository>();


var app = builder.Build();


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.MapGrpcService<GrpcProfileService>();

app.Run();
