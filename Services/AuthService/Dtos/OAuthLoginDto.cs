using System.ComponentModel.DataAnnotations;

namespace AuthService.AuthService.Application.Dtos;

public record OAuthLoginDto
{
    public required string Provider { get; init; }
    public required string IdToken { get; init; }
}
