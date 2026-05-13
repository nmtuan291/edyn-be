namespace AuthService.AuthService.Application.Dtos;

public record OAuthUserInfo
{
    public required string ProviderUserId { get; init; }
    public required string Email { get; init; }
    public required string Name { get; init; }
    public string? PictureUrl { get; init; }
}
