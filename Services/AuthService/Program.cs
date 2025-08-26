using AuthService.Data;
using AuthService.Entities;
using AuthService.Grpc;
using AuthService.Interfaces.Services;
using AuthService.Services;
using AuthService.Services.Sercurity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using UserService.Grpc;
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

builder.Services.AddGrpcClient<ProfileService.ProfileServiceClient>(option =>
{
    option.Address = new Uri("https://localhost:7189");
})
.ConfigurePrimaryHttpMessageHandler(() =>
{
    return new HttpClientHandler
    {
        ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
    };
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.MapGrpcService<GrpcAccountService>();

app.Run();