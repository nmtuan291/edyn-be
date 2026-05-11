using System.ComponentModel.DataAnnotations;

namespace AuthService.AuthService.Application.Dtos;

public record LogoutRequest
{
    [Required]
    public required string RefreshToken { get; init; }
}
