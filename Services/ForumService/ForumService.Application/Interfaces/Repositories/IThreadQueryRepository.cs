using ForumService.ForumService.Application.DTOs;
using ForumService.ForumService.Application.Requests;
using ForumService.ForumService.Domain.Entities;

namespace ForumService.ForumService.Application.Interfaces.Repositories;

public interface IThreadQueryRepository
{
    Task<List<ForumThread>> GetThreadsByForumIdAsync(ForumThreadPageQuery query, CancellationToken cancellationToken = default);
    Task<int> GetThreadCountByForumIdAsync(Guid forumId, CancellationToken cancellationToken = default);
    Task<ForumThread?> GetThreadByIdAsync(Guid threadId, Guid userId = default, CancellationToken cancellationToken = default);
    Task<List<ForumThread>> GetHomeFeedCandidatesAsync(List<Guid>? forumIds, int count, DateTime cutoff, CancellationToken cancellationToken = default);
    Task<List<ForumThread>> SearchThreadsAsync(string query, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<int> SearchThreadsCountAsync(string query, CancellationToken cancellationToken = default);
}
