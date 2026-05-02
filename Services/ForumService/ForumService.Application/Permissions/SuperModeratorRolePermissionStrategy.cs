using ForumService.ForumService.Application.Enums;

namespace ForumService.ForumService.Application.Permissions;

public sealed class SuperModeratorRolePermissionStrategy : IRolePermissionStrategy
{
    public ForumRole Role => ForumRole.SuperModerator;

    public ForumPermissionType GetDefaultPermissions() =>
        ForumPermissionType.ManageForumInfo | ForumPermissionType.ManageRoles |
        ForumPermissionType.PinThread | ForumPermissionType.LockThread |
        ForumPermissionType.DeleteThread | ForumPermissionType.EditAnyThread |
        ForumPermissionType.DeleteComment | ForumPermissionType.EditAnyComment |
        ForumPermissionType.BanMember | ForumPermissionType.ManageTags |
        ForumPermissionType.CreateThread | ForumPermissionType.CreateComment |
        ForumPermissionType.Vote;
}
