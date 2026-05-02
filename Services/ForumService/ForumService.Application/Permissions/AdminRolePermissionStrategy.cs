using ForumService.ForumService.Application.Enums;

namespace ForumService.ForumService.Application.Permissions;

public sealed class AdminRolePermissionStrategy : IRolePermissionStrategy
{
    public ForumRole Role => ForumRole.Admin;

    public ForumPermissionType GetDefaultPermissions() =>
        ForumPermissionType.All;
}
