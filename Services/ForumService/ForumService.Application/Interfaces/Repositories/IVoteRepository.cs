using ForumService.ForumService.Domain.Entities;

namespace ForumService.ForumService.Application.Interfaces.Repositories;

public interface IVoteRepository
{
    Task AddThreadVoteAsync(ThreadVote vote, Guid forumId);
    Task DeleteThreadVote(ThreadVote vote, Guid forumId);
    Task UpdateThreadVote(ThreadVote vote, Guid forumId);
    Task<ThreadVote?> GetThreadVoteAsync(Guid threadId, Guid userId);
    Task<int> CountUpvotesAsync(Guid threadId);
    Task<Dictionary<Guid, bool>> GetVotedThreadsAsync(Guid userId, Guid forumId);
}