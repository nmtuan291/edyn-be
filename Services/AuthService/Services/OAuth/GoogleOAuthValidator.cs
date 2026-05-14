using AuthService.AuthService.Application.Dtos;
using AuthService.Interfaces.Services;
using Google.Apis.Auth;

namespace AuthService.Services.OAuth;

public class GoogleOAuthValidator : IOAuthValidator
{
    private readonly IConfiguration _config;

    public string Provider => "google";

    public GoogleOAuthValidator(IConfiguration config)
    {
        _config = config;
    }

    public async Task ValidateTokenAsync(string token)
    {
        var clientId = _config["OAuth:Google:ClientId"]
                       ?? throw new InvalidOperationException("Google ClientId is not configured");

        var settings = new GoogleJsonWebSignature.ValidationSettings
        {
            Audience = new[] { clientId }
        };

        try
        {
            await GoogleJsonWebSignature.ValidateAsync(token, settings);
        }
        catch (InvalidJwtException)
        {
            throw new ArgumentException("Invalid Google ID token");
        }
    }

    public bool IsConfigured()
    {
        return !string.IsNullOrEmpty(_config["OAuth:Google:ClientId"]);
    }

    public async Task<OAuthUserInfo> GetUserInfoAsync(string token)
    {
        var clientId = _config["OAuth:Google:ClientId"]
                       ?? throw new InvalidOperationException("Google ClientId is not configured");

        var settings = new GoogleJsonWebSignature.ValidationSettings
        {
            Audience = new[] { clientId }
        };

        var payload = await GoogleJsonWebSignature.ValidateAsync(token, settings);

        return new OAuthUserInfo
        {
            ProviderUserId = payload.Subject,
            Email = payload.Email,
            Name = payload.Name ?? payload.Email.Split('@')[0],
            PictureUrl = payload.Picture
        };
    }
}
