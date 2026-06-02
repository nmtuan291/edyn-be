using System.ComponentModel.DataAnnotations;

namespace AuthService.AuthService.Application.Dtos;

public record RefreshTokenRequest
{
    [Required]
    [StringLength(4096)]
    public required string ExpiredToken { get; init; }

    [Required]
    [StringLength(1024, MinimumLength = 1)]
    public required string RefreshToken { get; init; }
}
