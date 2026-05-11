namespace ForumService.ForumService.Application.Requests;

public record EditCommentRequest
{
    public required string Content { get; init; }
}
