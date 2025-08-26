using ForumService.ForumService.Domain.Entities;

namespace ForumService.ForumService.Application.Interfaces.Repositories;

public interface IVoteRepository
{
    Task AddThreadVoteAsync(ThreadVote vote, Guid forumId);
    Task DeleteThreadVote(ThreadVote vote, Guid forumId);
    Task UpdateThreadVoteRedisAsync(Guid userId, Guid threadId, Guid forumId, bool downVote);
    Task<ThreadVote?> GetThreadVoteAsync(Guid threadId, Guid userId);
    Task<int> CountUpvotesAsync(Guid threadId);
    Task<Dictionary<Guid, bool>> GetVotedThreadsAsync(Guid userId, Guid forumId);
    Task RemoveThreadVoteRedisAsync(Guid userId, Guid threadId, Guid forumId);
}