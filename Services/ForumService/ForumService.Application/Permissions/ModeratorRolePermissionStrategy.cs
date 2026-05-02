using ForumService.ForumService.Application.Enums;

namespace ForumService.ForumService.Application.Permissions;

public sealed class ModeratorRolePermissionStrategy : IRolePermissionStrategy
{
    public ForumRole Role => ForumRole.Moderator;

    public ForumPermissionType GetDefaultPermissions() =>
        ForumPermissionType.PinThread | ForumPermissionType.LockThread |
        ForumPermissionType.DeleteThread | ForumPermissionType.DeleteComment |
        ForumPermissionType.BanMember | ForumPermissionType.ManageTags |
        ForumPermissionType.CreateThread | ForumPermissionType.CreateComment |
        ForumPermissionType.Vote;
}
