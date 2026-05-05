namespace ForumService.ForumService.Application.DTOs;

public record UserDto
{
    public Guid Id { get; init; }
    public string Username { get; init; } = string.Empty;
    public string Avatar { get; init; } = string.Empty;
    public string Bio { get; init; } = string.Empty;
    public DateTime BirthDay { get; init; }
    public int? Gender { get; init; }
}