namespace AuthService.Entities;

public class OAuthLogin
{
    public Guid Id { get; set; }
    public string Provider { get; set; } = string.Empty;
    public string ProviderUserId { get; set; } = string.Empty;
    public string AccountId { get; set; } = string.Empty;
    public Account Account { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
}
