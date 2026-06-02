using System.Security.Cryptography;

namespace AuthService.Entities;

public class RefreshToken
{
    public Guid Id { get; set; }
    public string Token { get; set; }
    public string AccountId { get; set; }
    public DateTime Expires { get; set; }
    public DateTime Created { get; set; }
    public bool Revoked { get; set; }
    public Account Account { get; set; }

    private RefreshToken() { }

    private RefreshToken(string accountId, int durationInDays, string token)
    {
        Id = Guid.NewGuid();
        Token = token;
        AccountId = accountId;
        Expires = DateTime.UtcNow.AddDays(durationInDays);
        Created = DateTime.UtcNow;
        Revoked = false;
    }

    public bool IsActive()
    {
        return Expires >= DateTime.UtcNow;
    }

    public static RefreshToken GenerateRefreshToken(string accountId, int durationInDays)
    {
        Span<byte> randomNumber = stackalloc byte[64];
        RandomNumberGenerator.Fill(randomNumber);
        
        string tokenString = Convert.ToBase64String(randomNumber);

        return new RefreshToken(accountId, durationInDays, tokenString);
    }

    public void UpdateRefreshToken(RefreshToken token)
    {
        Token = token.Token;
        Expires = token.Expires;
        Created = token.Created;
        Revoked = token.Revoked;
    }

    public bool ValidateToken(string token)
    {
        return Token == token;
    }
}