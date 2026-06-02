using System.ComponentModel.DataAnnotations;

namespace AuthService.AuthService.Application.Dtos;

public record OAuthLoginDto
{
    [Required]
    [StringLength(50)]
    public required string Provider { get; init; }

    [Required]
    [StringLength(8192)]
    public required string IdToken { get; init; }
}
