using ForumService.ForumService.Application.Enums;

namespace ForumService.ForumService.Application.DTOs;

public record ForumUserDto
{
    public Guid ForumId { get; init; }
    public string Name { get; init; }
    public string ForumImage { get; init; }
    public ForumRole Role { get; init; }
}