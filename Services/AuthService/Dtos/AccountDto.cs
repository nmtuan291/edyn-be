namespace AuthService.AuthService.Application.Dtos;

public class AccountDto
{
    public required string Id { get; set; }
    public required string UserName { get; set; }
    public required string Email { get; set; }
    public required bool IsActive { get; set; }
}