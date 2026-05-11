using AuthService.Data;
using AuthService.Entities;
using AuthService.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

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

    public async Task<RefreshToken?> GetRefreshTokenAsync(string accountId, string token)
    {
        return await _context.RefreshTokens
            .FirstOrDefaultAsync(t => t.AccountId == accountId && t.Token == token && !t.Revoked);
    }

    public async Task RevokeRefreshTokenAsync(string accountId, string token)
    {
        var refreshToken = await _context.RefreshTokens
            .FirstOrDefaultAsync(t => t.AccountId == accountId && t.Token == token);
        
        if (refreshToken != null)
        {
            refreshToken.Revoked = true;
            await _context.SaveChangesAsync();
        }
    }

    public async Task RevokeAllRefreshTokensAsync(string accountId)
    {
        var tokens = await _context.RefreshTokens
            .Where(t => t.AccountId == accountId && !t.Revoked)
            .ToListAsync();
        
        foreach (var token in tokens)
        {
            token.Revoked = true;
        }
        
        await _context.SaveChangesAsync();
    }
}