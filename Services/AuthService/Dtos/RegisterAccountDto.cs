using System.ComponentModel.DataAnnotations;

namespace AuthService.AuthService.Application.Dtos;

public class RegisterAccountDto
{
    [Required, MinLength(3), MaxLength(50)]
    public required string Username { get; set; }
    
    [Required, MinLength(6), MaxLength(100)]
    public required string Password { get; set; }
    
    [Required]
    public required string PasswordVerify { get; set; }
    
    [Required, EmailAddress]
    public required string Email { get; set; }
    
    public required int Gender { get; set; }
}
