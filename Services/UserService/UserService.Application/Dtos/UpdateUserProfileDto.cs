namespace UserService.UserService.Application.Dtos;

public record UpdateUserProfileDto
{
    public string? Bio { get; init; }
    public string? Avatar { get; init; }
    public DateTime? Birthday { get; init; }
    public int? Gender { get; init; }
}
