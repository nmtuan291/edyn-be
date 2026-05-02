using ForumService.ForumService.Application.Enums;

namespace ForumService.ForumService.Application.Permissions;

public interface IRolePermissionStrategy
{
    ForumRole Role { get; }
    ForumPermissionType GetDefaultPermissions();
}
