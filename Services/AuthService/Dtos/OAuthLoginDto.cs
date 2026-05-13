using System.ComponentModel.DataAnnotations;

namespace AuthService.AuthService.Application.Dtos;

public record OAuthLoginDto
{
    /// <summary>
    /// The OAuth2 provider: "google" or "facebook"
    /// </summary>
    [Required]
    public required string Provider { get; init; }
    
    /// <summary>
    /// The ID token (Google) or access token (Facebook) obtained from the frontend SDK
    /// </summary>
    [Required]
    public required string IdToken { get; init; }
}
