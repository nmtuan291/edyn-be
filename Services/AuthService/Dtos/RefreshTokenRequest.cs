namespace AuthService.AuthService.Application.Dtos;

public class RefreshTokenRequest
{
    public string ExpiredToken { get; set; }
    public string RefreshToken { get; set; }
}