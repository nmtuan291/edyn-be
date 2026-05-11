using AuthService.Entities;

namespace AuthService.Interfaces.Repositories;

public interface ITokenRepository
{
    Task SaveRefreshToken(RefreshToken token);
    Task SaveChangesAsync();
    Task<RefreshToken?> GetRefreshTokenAsync(string accountId, string token);
    Task RevokeRefreshTokenAsync(string accountId, string token);
    Task RevokeAllRefreshTokensAsync(string accountId);
}