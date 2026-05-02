using ForumService.ForumService.Application.DTOs;
using ForumService.ForumService.Application.Requests;
using ForumService.ForumService.Domain.Entities;

namespace ForumService.ForumService.Application.Interfaces.Repositories
{
    public interface IThreadRepository
    {
        Task<List<ForumThread>> GetThreadsByForumIdAsync(ForumThreadPageQuery query, CancellationToken cancellationToken = default);
        Task DeleteThreadByIdAsync(Guid threadId);
        Task InsertThreadAsync(ForumThread thread);
        void UpdateThread(ForumThread thread);
        Task<ForumThread?> GetThreadByIdAsync(Guid threadId, CancellationToken cancellationToken = default);
        Task<List<ForumThread>> GetHomeFeedCandidatesAsync(List<Guid>? forumIds, int count, DateTime cutoff, CancellationToken cancellationToken = default);
    }
}
