using AuthService.Data;
using AuthService.Entities;
using AuthService.Interfaces.Repositories;

namespace AuthService.Repositories;

public class TokenRepository : ITokenRepository
{
    private readonly AuthDbContext _context;

    public TokenRepository(AuthDbContext context)
    {
        _context = context;
    }

    public async Task SaveRefreshToken(RefreshToken token)
    {
        await _context.RefreshTokens.AddAsync(token);
    }
    
    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}