using ForumService.ForumService.Domain.Entities;

namespace ForumService.ForumService.Application.Interfaces.Repositories;

public interface IVoteQueryRepository
{
    Task<ThreadVote?> GetThreadVoteAsync(Guid threadId, Guid userId, CancellationToken cancellationToken = default);
    Task<CommentVote?> GetCommentVoteAsync(Guid commentId, Guid userId, CancellationToken cancellationToken = default);
    Task<int> CountUpvotesAsync(Guid threadId, CancellationToken cancellationToken = default);
    Task<Dictionary<Guid, bool>> GetVotedThreadsAsync(Guid userId, Guid forumId);
    Task<Dictionary<Guid, bool>> GetVotedCommentsAsync(Guid userId, Guid threadId);
    Task<Dictionary<string, double>> GetTagAffinityAsync(Guid userId);
}
