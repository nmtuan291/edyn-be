namespace ForumService.ForumService.Application.Requests;

public record CreateForumTagRequest
{
    public required string Name { get; init; }
    public string? Color { get; init; }
}
