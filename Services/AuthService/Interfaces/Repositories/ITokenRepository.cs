using AuthService.Entities;

namespace AuthService.Interfaces.Repositories;

public interface ITokenRepository
{
    Task SaveRefreshToken(RefreshToken token);
    Task SaveChangesAsync();
}