using AuthService.AuthService.Application.Dtos;

namespace AuthService.Interfaces.Services;

public interface IOAuthValidator
{
    /// <summary>
    /// The provider name this validator handles (e.g. "google", "facebook")
    /// </summary>
    string Provider { get; }

    /// <summary>
    /// Validates the token from the frontend SDK. Throws if invalid.
    /// </summary>
    Task ValidateTokenAsync(string token);

    /// <summary>
    /// Checks if the validator has all necessary configuration to function.
    /// </summary>
    bool IsConfigured();

    /// <summary>
    /// Fetches user information from the provider using the token.
    /// Should be called after ValidateTokenAsync succeeds.
    /// </summary>
    Task<OAuthUserInfo> GetUserInfoAsync(string token);
}
