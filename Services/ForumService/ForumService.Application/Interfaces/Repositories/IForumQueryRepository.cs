using ForumService.ForumService.Application.DTOs;
using ForumService.ForumService.Domain.Entities;

namespace ForumService.ForumService.Application.Interfaces.Repositories;

public interface IForumQueryRepository
{
    Task<Forum?> GetForumByIdAsync(Guid forumId, CancellationToken cancellationToken = default);
    Task<List<Forum>> GetForumsAsync(CancellationToken cancellationToken = default);
    Task<Forum?> GetForumByNameAsync(Guid userId, string name, CancellationToken cancellationToken = default);
    Task<ForumUser?> GetForumUserAsync(Guid forumId, Guid userId, CancellationToken cancellationToken = default);
    Task<List<ForumUser>> GetJoinedForumsByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<List<ForumUser>> GetForumUsersAsync(Guid forumId, CancellationToken cancellationToken = default);
    Task<List<Guid>> GetJoinedForumIdsAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<Dictionary<Guid, (string Name, string Image)>> GetForumInfoByIdsAsync(IEnumerable<Guid> forumIds, CancellationToken cancellationToken = default);
    Task<List<ForumTagDto>> GetForumTagCatalogAsync(Guid forumId, CancellationToken cancellationToken = default);
    Task<List<Forum>> SearchForumsAsync(string query, CancellationToken cancellationToken = default);
    ValueTask<List<Forum>> GetRecentVisitForumsAsync(Guid userId, CancellationToken cancellationToken = default);
}
