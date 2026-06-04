namespace ForumService.ForumService.Application.Requests;

public record PollVoteRequest
{
    public Guid ThreadId { get; init; }
    public string PollContent { get; init; } = string.Empty;
}
