namespace AuthService.AuthService.Application.Interfaces.Services;

public interface ITokenService
{
    string GenerateJwtToken(string userId, string email, string username);
}