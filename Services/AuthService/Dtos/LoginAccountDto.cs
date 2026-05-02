using System.ComponentModel.DataAnnotations;

namespace AuthService.AuthService.Application.Dtos;

public class LoginAccountDto
{
    [Required]
    public required string Username { get; set; }
    
    [Required]
    public required string Password { get; set; }
    
    public required bool IsEmail { get; set; }
}
