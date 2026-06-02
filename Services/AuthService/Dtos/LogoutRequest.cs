using System.ComponentModel.DataAnnotations;

namespace AuthService.AuthService.Application.Dtos;

public record LogoutRequest
{
    [Required]
    [StringLength(1024, MinimumLength = 1)]
    public required string RefreshToken { get; init; }
}
