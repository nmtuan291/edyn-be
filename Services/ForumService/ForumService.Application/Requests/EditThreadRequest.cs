namespace ForumService.ForumService.Application.Requests;

public record EditThreadRequest
{
    public string? Title { get; init; }
    public string? Content { get; init; }
}
