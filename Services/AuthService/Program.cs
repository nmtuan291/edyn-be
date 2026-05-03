using AuthService.Data;
using AuthService.Entities;
using AuthService.Grpc;
using AuthService.Interfaces.Repositories;
using AuthService.Interfaces.Services;
using AuthService.Repositories;
using AuthService.Services;
using AuthService.Middlewares;
using AuthService.Services.Security;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using UserService.Grpc;
using Scalar.AspNetCore;
using AccountService = AuthService.Services.AccountService;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Learn more about configuring OpenAPI at https://akSa.ms/aspnet/openapi
builder.Services.AddOpenApi();

// Add gRPC
builder.Services.AddGrpc();

builder.Services.AddDbContext<AuthDbContext>(options => options.UseNpgsql(builder.Configuration.GetConnectionString("WebApiDatabase")));

builder.Services.AddIdentity<Account, IdentityRole>()
    .AddEntityFrameworkStores<AuthDbContext>()
    .AddDefaultTokenProviders();

// Add dependency
builder.Services.AddScoped<IAccountService, AccountService>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddSingleton<RsaKeyProvider>();
builder.Services.AddScoped<ITokenRepository, TokenRepository>();

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
    var db = scope.ServiceProvider.GetRequiredService<AuthDbContext>();
    db.Database.Migrate();
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseMiddleware<ErrorHandlingMiddleware>();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.MapGrpcService<GrpcAccountService>();

app.Run();