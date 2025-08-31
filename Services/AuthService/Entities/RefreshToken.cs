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

    public bool IsActive()
    {
        return Expires >= DateTime.UtcNow;
    }

    public static RefreshToken GenerateRefreshToken(string accountId, int durationInDays)
    {
        var randomNumber = new byte[64];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(randomNumber);
        }

        RefreshToken token = new()
        {
            Id = Guid.NewGuid(),
            Token = Convert.ToBase64String(randomNumber),
            AccountId = accountId,
            Expires = DateTime.UtcNow.AddDays(durationInDays),
            Created = DateTime.UtcNow,
            Revoked = false
        };
        
        return token;
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