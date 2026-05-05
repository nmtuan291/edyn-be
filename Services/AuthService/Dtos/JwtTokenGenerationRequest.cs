using Microsoft.IdentityModel.Tokens;

namespace AuthService.Dtos;

public record JwtTokenGenerationRequest
{
    public required string UserId { get; init; }
    public required string Email { get; init; }
    public required string Username { get; init; }
    public required RsaSecurityKey PrivateKey { get; init; }
}
