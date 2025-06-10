namespace AuthService.AuthService.Application.Dtos;

public class LoginResponseDto
{
    public required string Id { get; set; }
    public required string UserName { get; set; }
    public required string Email { get; set; }
    public required string Token { get; set; }
}