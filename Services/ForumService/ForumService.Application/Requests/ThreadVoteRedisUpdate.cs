namespace ForumService.ForumService.Application.Requests;

public record ThreadVoteRedisUpdate(
    Guid UserId,
    Guid ThreadId,
    Guid ForumId,
    bool IsDownVote);
