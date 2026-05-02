using ForumService.ForumService.Application.Requests;
using ForumService.ForumService.Domain.Entities;

namespace ForumService.ForumService.Application.Interfaces.Repositories;

public interface IVoteRepository
{
    Task AddThreadVoteAsync(ThreadVote vote, Guid forumId);
    Task DeleteThreadVote(ThreadVote vote, Guid forumId);
    Task UpdateThreadVoteRedisAsync(ThreadVoteRedisUpdate update);
    Task<ThreadVote?> GetThreadVoteAsync(Guid threadId, Guid userId, CancellationToken cancellationToken = default);
    Task<int> CountUpvotesAsync(Guid threadId, CancellationToken cancellationToken = default);
    Task<Dictionary<Guid, bool>> GetVotedThreadsAsync(Guid userId, Guid forumId);
    Task RemoveThreadVoteRedisAsync(Guid userId, Guid threadId, Guid forumId);
    Task AddCommentVoteAsync(CommentVote vote, Guid threadId);
    Task<CommentVote?> GetCommentVoteAsync(Guid commentId, Guid userId, CancellationToken cancellationToken = default);
    Task<Dictionary<Guid, bool>> GetVotedCommentsAsync(Guid userId, Guid threadId);
    Task UpdateCommentVoteRedisAsync(CommentVoteRedisUpdate update);
    Task RemoveCommentVoteRedisAsync(Guid userId, Guid commentId, Guid threadId);
    Task IncrementTagAffinityAsync(Guid userId, IEnumerable<string> tagNames);
    Task DecrementTagAffinityAsync(Guid userId, IEnumerable<string> tagNames);
    Task<Dictionary<string, double>> GetTagAffinityAsync(Guid userId);
}