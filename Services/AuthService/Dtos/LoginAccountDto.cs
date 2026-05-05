using System.ComponentModel.DataAnnotations;

namespace AuthService.AuthService.Application.Dtos;

public record LoginAccountDto
{
    [Required]
    public required string Username { get; init; }
    
    [Required]
    public required string Password { get; init; }
    
    public required bool IsEmail { get; init; }
}
