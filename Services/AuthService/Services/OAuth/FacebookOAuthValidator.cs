using System.Text.Json;
using AuthService.AuthService.Application.Dtos;
using AuthService.Interfaces.Services;

namespace AuthService.Services.OAuth;

public class FacebookOAuthValidator : IOAuthValidator
{
    private readonly IConfiguration _config;
    private readonly IHttpClientFactory _httpClientFactory;

    public string Provider => "facebook";

    public FacebookOAuthValidator(IConfiguration config, IHttpClientFactory httpClientFactory)
    {
        _config = config;
        _httpClientFactory = httpClientFactory;
    }

    public async Task ValidateTokenAsync(string token)
    {
        var appId = _config["OAuth:Facebook:AppId"]
                    ?? throw new InvalidOperationException("Facebook AppId is not configured");
        var appSecret = _config["OAuth:Facebook:AppSecret"]
                        ?? throw new InvalidOperationException("Facebook AppSecret is not configured");

        var client = _httpClientFactory.CreateClient();

        var debugUrl = $"https://graph.facebook.com/debug_token?input_token={token}&access_token={appId}|{appSecret}";
        var debugResponse = await client.GetAsync(debugUrl);
        debugResponse.EnsureSuccessStatusCode();

        var debugContent = await debugResponse.Content.ReadAsStringAsync();
        using var debugDoc = JsonDocument.Parse(debugContent);
        var data = debugDoc.RootElement.GetProperty("data");

        if (!data.GetProperty("is_valid").GetBoolean())
            throw new ArgumentException("Invalid Facebook access token");

        var tokenAppId = data.GetProperty("app_id").GetString();
        if (tokenAppId != appId)
            throw new ArgumentException("Facebook token does not belong to this application");
    }

    public bool IsConfigured()
    {
        return !string.IsNullOrEmpty(_config["OAuth:Facebook:AppId"]) &&
               !string.IsNullOrEmpty(_config["OAuth:Facebook:AppSecret"]);
    }

    public async Task<OAuthUserInfo> GetUserInfoAsync(string token)
    {
        var client = _httpClientFactory.CreateClient();

        var meUrl = $"https://graph.facebook.com/me?fields=id,name,email,picture&access_token={token}";
        var meResponse = await client.GetAsync(meUrl);
        meResponse.EnsureSuccessStatusCode();

        var meContent = await meResponse.Content.ReadAsStringAsync();
        using var meDoc = JsonDocument.Parse(meContent);
        var root = meDoc.RootElement;

        var email = root.TryGetProperty("email", out var emailProp) ? emailProp.GetString() : null;
        if (string.IsNullOrEmpty(email))
            throw new ArgumentException("Facebook account does not have an email address. Email permission is required.");

        string? pictureUrl = null;
        if (root.TryGetProperty("picture", out var pictureProp) &&
            pictureProp.TryGetProperty("data", out var pictureData) &&
            pictureData.TryGetProperty("url", out var urlProp))
        {
            pictureUrl = urlProp.GetString();
        }

        return new OAuthUserInfo
        {
            ProviderUserId = root.GetProperty("id").GetString()!,
            Email = email,
            Name = root.GetProperty("name").GetString() ?? email.Split('@')[0],
            PictureUrl = pictureUrl
        };
    }
}
