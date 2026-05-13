using AuthService.AuthService.Application.Dtos;

namespace AuthService.Interfaces.Services;

/// <summary>
/// Base interface for all authentication services.
/// Contains shared session/token management operations.
/// </summary>
public interface IAuthService
{
    Task<TokenResponse> RefreshToken(string accountId, string refreshToken);
    Task Logout(string accountId, string refreshToken);
    Task RevokeAllSessions(string accountId);
}
