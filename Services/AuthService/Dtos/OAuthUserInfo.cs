namespace AuthService.AuthService.Application.Dtos;

/// <summary>
/// Represents the validated user information extracted from an OAuth2 provider
/// </summary>
public record OAuthUserInfo
{
    public required string ProviderUserId { get; init; }
    public required string Email { get; init; }
    public required string Name { get; init; }
    public string? PictureUrl { get; init; }
}
