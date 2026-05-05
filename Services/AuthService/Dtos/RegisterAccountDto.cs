using System.ComponentModel.DataAnnotations;

namespace AuthService.AuthService.Application.Dtos;

public record RegisterAccountDto
{
    [Required, MinLength(3), MaxLength(50)]
    public required string Username { get; init; }
    
    [Required, MinLength(6), MaxLength(100)]
    public required string Password { get; init; }
    
    [Required]
    public required string PasswordVerify { get; init; }
    
    [Required, EmailAddress]
    public required string Email { get; init; }
    
    public required int Gender { get; init; }
}
