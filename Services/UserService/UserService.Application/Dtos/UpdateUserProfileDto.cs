using System.ComponentModel.DataAnnotations;

namespace UserService.UserService.Application.Dtos;

public record UpdateUserProfileDto
{
    [StringLength(1000)]
    public string? Bio { get; init; }

    [Url]
    [StringLength(2048)]
    public string? Avatar { get; init; }

    public DateTime? Birthday { get; init; }
    public int? Gender { get; init; }
}
