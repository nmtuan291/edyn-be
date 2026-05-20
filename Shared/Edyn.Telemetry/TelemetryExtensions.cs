using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using System;

namespace Edyn.Telemetry;

public static class TelemetryExtensions
{
    public static IServiceCollection AddEdynTelemetry(
        this IServiceCollection services,
        IConfiguration configuration,
        string serviceName,
        Action<TracerProviderBuilder>? configureTracing = null)
    {
        var otlpEndpoint = configuration["OpenTelemetry:OtlpEndpoint"];

        if (string.IsNullOrEmpty(otlpEndpoint))
            return services;

        services.AddOpenTelemetry()
            .ConfigureResource(resource => resource
                .AddService(
                    serviceName: serviceName,
                    serviceVersion: "1.0.0"))
            .WithTracing(tracing =>
            {
                tracing
                    .AddAspNetCoreInstrumentation(options =>
                    {
                        // Filter out health checks, OpenAPI/Scalar, and noise
                        options.Filter = httpContext =>
                            !httpContext.Request.Path.StartsWithSegments("/health") &&
                            !httpContext.Request.Path.StartsWithSegments("/openapi") &&
                            !httpContext.Request.Path.StartsWithSegments("/scalar") &&
                            !httpContext.Request.Path.StartsWithSegments("/_framework");
                    })
                    .AddHttpClientInstrumentation()
                    .AddOtlpExporter(otlp =>
                    {
                        otlp.Endpoint = new Uri(otlpEndpoint);
                    });

                // Allow each service to add its own custom instrumentations (e.g. EF Core, Redis, gRPC)
                configureTracing?.Invoke(tracing);
            });

        return services;
    }
}
