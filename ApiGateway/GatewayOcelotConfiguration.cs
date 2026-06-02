using Newtonsoft.Json;
using Ocelot;
using Ocelot.Configuration.File;
using Ocelot.DependencyInjection;

namespace ApiGateway;

public static class GatewayOcelotConfiguration
{
    private static readonly (string Prefix, string Service)[] UpstreamPrefixToService =
    [
        ("/hubs/notification", "Notification"),
        ("/hubs/chat", "Chat"),
        ("/users", "User"),
        ("/forumthread", "Forum"),
        ("/forum", "Forum"),
        ("/feed", "Forum"),
        ("/auth", "Auth"),
        ("/.well-known", "Auth"),
        ("/notifications", "Notification"),
        ("/chat", "Chat"),
    ];

    public static FileConfiguration LoadMerged(IConfiguration configuration, IWebHostEnvironment env)
    {
        var path = Path.Combine(env.ContentRootPath, "ocelot.json");
        if (!File.Exists(path))
            throw new FileNotFoundException("Ocelot configuration not found.", path);

        var json = File.ReadAllText(path);
        var fileConfig = JsonConvert.DeserializeObject<FileConfiguration>(json)
            ?? throw new InvalidOperationException("Failed to deserialize ocelot.json.");

        ApplyDownstreamOverrides(fileConfig, configuration);
        ApplyPublicBaseUrl(fileConfig, configuration);
        return fileConfig;
    }

    private static void ApplyPublicBaseUrl(FileConfiguration fileConfig, IConfiguration configuration)
    {
        var baseUrl = configuration["Gateway:PublicBaseUrl"];
        if (string.IsNullOrWhiteSpace(baseUrl))
            return;

        fileConfig.GlobalConfiguration ??= new FileGlobalConfiguration();
        fileConfig.GlobalConfiguration.BaseUrl = baseUrl.Trim();
    }

    private static void ApplyDownstreamOverrides(FileConfiguration fileConfig, IConfiguration configuration)
    {
        var downstream = configuration.GetSection("Gateway:Downstream");
        if (!downstream.GetChildren().Any())
            return;

        foreach (var route in fileConfig.Routes)
        {
            var service = ResolveServiceName(route.UpstreamPathTemplate);
            if (service is null)
                continue;

            var section = downstream.GetSection(service);
            var host = section["Host"];
            if (string.IsNullOrWhiteSpace(host))
                continue;

            var scheme = string.IsNullOrWhiteSpace(section["Scheme"]) ? "https" : section["Scheme"]!.Trim();
            var port = 443;
            if (int.TryParse(section["Port"], out var p) && p > 0)
                port = p;

            route.DownstreamScheme = scheme;
            route.DownstreamHostAndPorts =
            [
                new FileHostAndPort { Host = host.Trim(), Port = port }
            ];
        }
    }

    private static string? ResolveServiceName(string? upstreamPathTemplate)
    {
        if (string.IsNullOrEmpty(upstreamPathTemplate))
            return null;

        foreach (var (prefix, service) in UpstreamPrefixToService)
        {
            if (upstreamPathTemplate.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)
                && (upstreamPathTemplate.Length == prefix.Length || upstreamPathTemplate[prefix.Length] == '/'))
                return service;
        }

        return null;
    }

    public static void AddOcelotFromMerged(
        IConfigurationBuilder configurationBuilder,
        FileConfiguration merged,
        IWebHostEnvironment env)
    {
        configurationBuilder.AddOcelot(merged, env, MergeOcelotJson.ToMemory);
    }
}
