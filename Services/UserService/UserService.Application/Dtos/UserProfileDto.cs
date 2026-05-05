namespace UserService.UserService.Application.Dtos;

public record UserProfileDto
{
    public string? AccountId { get; init; }
    public required string UserName { get; init; }
    public DateTime? Birthday { get; init; }
    public required string Avatar { get; init; }
    public string? Bio { get; init; } 
    public int? Gender { get; init; }
}