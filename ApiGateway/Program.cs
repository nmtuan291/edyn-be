using System.IdentityModel.Tokens.Jwt;
using ApiGateway;
using Microsoft.IdentityModel.Tokens;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using Scalar.AspNetCore;
using Edyn.Telemetry;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddEdynTelemetry(builder.Configuration, "edyn-api-gateway");


var jwtAudience = builder.Configuration["Jwt:Audience"];
builder.Services.AddAuthentication("Bearer")
    .AddJwtBearer("Bearer", options =>
    {
        options.Authority = builder.Configuration["Jwt:Authority"];
        options.RequireHttpsMetadata = !string.Equals(
            builder.Configuration["Jwt:RequireHttpsMetadata"],
            "false",
            StringComparison.OrdinalIgnoreCase);
        options.TokenValidationParameters = new TokenValidationParameters
        {
            NameClaimType = JwtRegisteredClaimNames.Name,
            RoleClaimType = "role",
            ValidateAudience = !string.IsNullOrWhiteSpace(jwtAudience),
            ValidAudience = string.IsNullOrWhiteSpace(jwtAudience) ? null : jwtAudience,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"]
        };
    });

var ocelotMerged = GatewayOcelotConfiguration.LoadMerged(builder.Configuration, builder.Environment);
GatewayOcelotConfiguration.AddOcelotFromMerged(
    (IConfigurationBuilder)builder.Configuration,
    ocelotMerged,
    builder.Environment);
builder.Services.AddOcelot(builder.Configuration);

var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend",
        policy => policy
            .WithOrigins(allowedOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials());
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}
app.UseHttpsRedirection();
app.UseCors("AllowFrontend");
app.UseAuthentication();
app.UseAuthorization();
app.UseWebSockets();

await app.UseOcelot();

app.Run();

