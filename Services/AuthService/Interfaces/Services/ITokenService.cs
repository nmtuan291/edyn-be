using Microsoft.IdentityModel.Tokens;

namespace AuthService.Interfaces.Services;

public interface ITokenService
{
    string GenerateJwtToken(string userId, string email, string username, RsaSecurityKey privateKey);
}