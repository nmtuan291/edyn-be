using ForumService.ForumService.Application.DTOs;
using ForumService.ForumService.Application.Requests;

namespace ForumService.ForumService.Application.Interfaces.Services
{
    public interface IForumService
    {
        Task<ForumDto?> AddForum(ForumDto forum, string userId);
        Task<ForumDto?> GetForum(string forumName, CancellationToken cancellationToken = default);
        Task<List<ForumDto>> GetForums(CancellationToken cancellationToken = default);
        Task AddUserToForum(Guid forumId, Guid userId);
        Task<MemberPermissionDto?> GetUserPermission(Guid forumId, Guid userId, CancellationToken cancellationToken = default);
        Task<List<ForumUserDto>> GetJoinedForums(Guid userId, CancellationToken cancellationToken = default);
        Task<List<ForumMemberDto>> GetForumMembers(Guid forumId, CancellationToken cancellationToken = default);
        Task RemoveForumMember(Guid forumId, Guid targetUserId, Guid actingUserId);
        Task LeaveForumAsync(Guid forumId, Guid userId);

        Task<List<ForumTagDto>> GetForumTagsAsync(Guid forumId, CancellationToken cancellationToken = default);
        Task<ForumTagDto> CreateForumTagAsync(Guid forumId, CreateForumTagRequest request, Guid actingUserId);
        Task<List<ForumDto>> SearchForums(string query, CancellationToken cancellationToken = default);
    }
}