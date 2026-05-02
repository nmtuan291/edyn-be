namespace ForumService.ForumService.Application.Requests;

public record CommentVoteRedisUpdate(
    Guid UserId,
    Guid CommentId,
    Guid ThreadId,
    bool IsDownVote);
