namespace AuthService.AuthService.Application.Dtos;

public class TokenResponse
{
    public string AccessToken { get; set; }
    public string RefreshToken { get; set; }
}