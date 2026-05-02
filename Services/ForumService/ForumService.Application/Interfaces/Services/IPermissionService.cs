using ForumService.ForumService.Application.Enums;
using ForumService.ForumService.Application.Requests;

namespace ForumService.ForumService.Application.Interfaces.Services;

public interface IPermissionService
{
    ForumPermissionType GetDefaultPermissions(ForumRole role);
    Task<bool> HasPermissionAsync(Guid forumId, Guid userId, ForumPermissionType required, CancellationToken cancellationToken = default);
    Task<ForumPermissionType> GetEffectivePermissionsAsync(Guid forumId, Guid userId, CancellationToken cancellationToken = default);
    Task SetUserRoleAsync(ForumMemberRoleUpdate update);
    Task SetPermissionOverridesAsync(ForumMemberPermissionOverridesUpdate update);
    Task InvalidateCacheAsync(Guid forumId, Guid userId);
}
