using System.ComponentModel.DataAnnotations;

namespace AuthService.AuthService.Application.Dtos;

public record RefreshTokenRequest
{
    [Required]
    public required string ExpiredToken { get; init; }
    
    [Required]
    public required string RefreshToken { get; init; }
}
