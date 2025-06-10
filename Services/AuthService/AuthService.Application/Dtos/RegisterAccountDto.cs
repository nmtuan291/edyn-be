namespace AuthService.AuthService.Application.Dtos;

public class RegisterAccountDto
{
    public required string UserName { get; set; }
    public required string Password { get; set; }
    public required string PasswordVerify { get; set; }
    public required string Email { get; set; }
}