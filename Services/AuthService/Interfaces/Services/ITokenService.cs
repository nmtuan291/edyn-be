using AuthService.Dtos;

namespace AuthService.Interfaces.Services;

public interface ITokenService
{
    string GenerateJwtToken(JwtTokenGenerationRequest request);
}