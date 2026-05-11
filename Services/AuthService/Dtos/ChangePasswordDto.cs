using System.ComponentModel.DataAnnotations;

namespace AuthService.AuthService.Application.Dtos;

public record ChangePasswordDto
{
    [Required]
    public required string CurrentPassword { get; init; }
    
    [Required, MinLength(6), MaxLength(100)]
    public required string NewPassword { get; init; }
    
    [Required]
    public required string NewPasswordVerify { get; init; }
}
