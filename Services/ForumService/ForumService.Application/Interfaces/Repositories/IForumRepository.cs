using ForumService.ForumService.Application.DTOs;
using ForumService.ForumService.Application.Enums;
using ForumService.ForumService.Domain.Entities;

namespace ForumService.ForumService.Application.Interfaces.Repositories
{
    public interface IForumRepository
    {
        Task<Forum?> InsertForumAsync(Forum forum);
        Task<Forum?> GetForumByIdAsync(Guid forumId, CancellationToken cancellationToken = default);
        Task<List<Forum>> GetForumsAsync(CancellationToken cancellationToken = default);
        Task InsertUserToForumAsync(Guid forumId, Guid userId, ForumRole role);
        Task<Forum?> GetForumByNameAsync(Guid userId, string name, CancellationToken cancellationToken = default);
        Task<ForumUser?> GetForumUserAsync(Guid forumId, Guid userId, CancellationToken cancellationToken = default);
        Task UpdateForumUserAsync(ForumUser forumUser);
        Task RemoveForumUserAsync(Guid forumId, Guid userId);
        Task<List<ForumUser>> GetJoinedForumsByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
        Task<List<ForumUser>> GetForumUsersAsync(Guid forumId, CancellationToken cancellationToken = default);
        Task<List<Guid>> GetJoinedForumIdsAsync(Guid userId, CancellationToken cancellationToken = default);
        Task<Dictionary<Guid, (string Name, string Image)>> GetForumInfoByIdsAsync(IEnumerable<Guid> forumIds, CancellationToken cancellationToken = default);

        Task SetCachedPermissionsAsync(Guid forumId, Guid userId, int permissions);
        ValueTask<int?> GetCachedPermissionsAsync(Guid forumId, Guid userId);
        Task InvalidateCachedPermissionsAsync(Guid forumId, Guid userId);

        Task<List<ForumTagDto>> GetForumTagCatalogAsync(Guid forumId, CancellationToken cancellationToken = default);
        Task<bool> AddForumTagCatalogIfNotExistsAsync(Guid forumId, string name, string color);
        Task<List<Forum>> SearchForumsAsync(string query, CancellationToken cancellationToken = default);
        ValueTask<List<Forum>> GetRecentVisitForumsAsync(Guid userId, CancellationToken cancellationToken = default);
    }
}
