using ForumService.ForumService.Application.Enums;

namespace ForumService.ForumService.Application.Permissions;

public sealed class MemberRolePermissionStrategy : IRolePermissionStrategy
{
    public ForumRole Role => ForumRole.Member;

    public ForumPermissionType GetDefaultPermissions() =>
        ForumPermissionType.CreateThread | ForumPermissionType.CreateComment |
        ForumPermissionType.Vote;
}
