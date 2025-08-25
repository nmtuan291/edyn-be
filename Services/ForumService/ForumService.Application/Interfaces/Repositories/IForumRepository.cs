using ForumService.ForumService.Application.DTOs;
using ForumService.ForumService.Domain.Entities;

namespace ForumService.ForumService.Application.Interfaces.Repositories
{
    public interface IForumRepository
    {
        Task<Forum?> InsertForumAsync(Forum forum);
        Task<Forum?> GetForumByIdAsync(Guid forumId);
        Task<List<Forum>> GetForumsAsync();
        Task InsertUserToForumAsync(Guid forumId, Guid userId, string permissions,  bool isModerator);
        Task<Forum?> GetForumByNameAsync(string name);
        Task<string?> GetUserPermissionAsync(Guid forumId, Guid userId);
        Task<List<ForumUser>> GetJoinedForumsByUserIdAsync(Guid userId);
    }
}
