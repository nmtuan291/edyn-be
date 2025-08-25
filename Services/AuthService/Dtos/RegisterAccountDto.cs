namespace AuthService.AuthService.Application.Dtos;

public class RegisterAccountDto
{
    public required string Username { get; set; }
    public required string Password { get; set; }
    public required string PasswordVerify { get; set; }
    public required string Email { get; set; }
    public required int Gender { get; set; }
}