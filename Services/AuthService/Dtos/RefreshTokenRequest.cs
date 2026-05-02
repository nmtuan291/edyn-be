using System.ComponentModel.DataAnnotations;

namespace AuthService.AuthService.Application.Dtos;

public class RefreshTokenRequest
{
    [Required]
    public required string ExpiredToken { get; set; }
    
    [Required]
    public required string RefreshToken { get; set; }
}
