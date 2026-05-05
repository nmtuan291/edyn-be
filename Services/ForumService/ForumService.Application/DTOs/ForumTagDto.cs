namespace ForumService.ForumService.Application.DTOs;

public record ForumTagDto
{
    public int Id { get; init; }
    public Guid ForumId { get; init; }
    public required string Name { get; init; }
    public required string Color { get; init; }
}
