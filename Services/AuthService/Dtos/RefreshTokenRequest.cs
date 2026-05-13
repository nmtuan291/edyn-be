using System.ComponentModel.DataAnnotations;

namespace AuthService.AuthService.Application.Dtos;

public record RefreshTokenRequest
{
    public required string ExpiredToken { get; init; }
    public required string RefreshToken { get; init; }
}
