using ForumService.ForumService.Application.Enums;
using ForumService.ForumService.Application.Interfaces.Services;
using ForumService.ForumService.Application.Interfaces.UnitOfWork;
using ForumService.ForumService.Application.Permissions;
using ForumService.ForumService.Application.Requests;

namespace ForumService.ForumService.Application;

public class PermissionService : IPermissionService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IReadOnlyDictionary<ForumRole, IRolePermissionStrategy> _strategiesByRole;

    public PermissionService(
        IUnitOfWork unitOfWork,
        IEnumerable<IRolePermissionStrategy> rolePermissionStrategies)
    {
        _unitOfWork = unitOfWork;
        _strategiesByRole = RolePermissionStrategyRegistry.Build(rolePermissionStrategies);
    }

    public ForumPermissionType GetDefaultPermissions(ForumRole role)
    {
        return _strategiesByRole.TryGetValue(role, out var strategy)
            ? strategy.GetDefaultPermissions()
            : ForumPermissionType.None;
    }

    public async Task<ForumPermissionType> GetEffectivePermissionsAsync(Guid forumId, Guid userId, CancellationToken cancellationToken = default)
    {
        var cached = await _unitOfWork.ForumRepo.GetCachedPermissionsAsync(forumId, userId);
        if (cached.HasValue)
            return (ForumPermissionType)cached.Value;

        var forumUser = await _unitOfWork.ForumRepo.GetForumUserAsync(forumId, userId, cancellationToken);
        if (forumUser == null || !forumUser.Active)
            return ForumPermissionType.None;

        var permissions = ResolvePermissions(forumUser);

        await _unitOfWork.ForumRepo.SetCachedPermissionsAsync(forumId, userId, (int)permissions);
        return permissions;
    }

    public async Task<bool> HasPermissionAsync(Guid forumId, Guid userId, ForumPermissionType required, CancellationToken cancellationToken = default)
    {
        var effective = await GetEffectivePermissionsAsync(forumId, userId, cancellationToken);
        return (effective & required) == required;
    }

    public async Task SetUserRoleAsync(ForumMemberRoleUpdate update)
    {
        var actingUser = await _unitOfWork.ForumRepo.GetForumUserAsync(update.ForumId, update.ActingUserId);
        if (actingUser == null)
            throw new UnauthorizedAccessException("You are not a member of this forum.");

        var actingPerms = ResolvePermissions(actingUser);
        if (!actingPerms.HasFlag(ForumPermissionType.ManageRoles))
            throw new UnauthorizedAccessException("You do not have permission to manage roles.");

        var targetUser = await _unitOfWork.ForumRepo.GetForumUserAsync(update.ForumId, update.TargetUserId);
        if (targetUser == null)
            throw new InvalidOperationException("Target user is not a member of this forum.");

        if (targetUser.Role == ForumRole.Admin)
            throw new InvalidOperationException("Cannot change the role of the forum admin.");

        if (update.NewRole == ForumRole.Admin)
            throw new InvalidOperationException("Cannot promote a user to admin.");

        // SuperModerators can only manage Moderator and Member roles
        if (actingUser.Role == ForumRole.SuperModerator && update.NewRole == ForumRole.SuperModerator)
            throw new UnauthorizedAccessException("SuperModerators cannot promote to SuperModerator.");

        if (actingUser.Role == ForumRole.SuperModerator && targetUser.Role == ForumRole.SuperModerator)
            throw new UnauthorizedAccessException("SuperModerators cannot manage other SuperModerators.");

        targetUser.Role = update.NewRole;
        targetUser.PermissionOverrides = null;
        await _unitOfWork.ForumRepo.UpdateForumUserAsync(targetUser);
        await _unitOfWork.CommitAsync();
        await _unitOfWork.ForumRepo.InvalidateCachedPermissionsAsync(update.ForumId, update.TargetUserId);
    }

    public async Task SetPermissionOverridesAsync(ForumMemberPermissionOverridesUpdate update)
    {
        var actingUser = await _unitOfWork.ForumRepo.GetForumUserAsync(update.ForumId, update.ActingUserId);
        if (actingUser == null)
            throw new UnauthorizedAccessException("You are not a member of this forum.");

        var actingPerms = ResolvePermissions(actingUser);
        if (!actingPerms.HasFlag(ForumPermissionType.ManageRoles))
            throw new UnauthorizedAccessException("You do not have permission to manage roles.");

        var targetUser = await _unitOfWork.ForumRepo.GetForumUserAsync(update.ForumId, update.TargetUserId);
        if (targetUser == null)
            throw new InvalidOperationException("Target user is not a member of this forum.");

        if (targetUser.Role == ForumRole.Admin)
            throw new InvalidOperationException("Cannot override permissions of the forum admin.");

        // Prevent granting permissions the acting user doesn't have themselves
        if ((update.Overrides & ~actingPerms) != ForumPermissionType.None)
            throw new UnauthorizedAccessException("Cannot grant permissions you do not have.");

        targetUser.PermissionOverrides = (int)update.Overrides;
        await _unitOfWork.ForumRepo.UpdateForumUserAsync(targetUser);
        await _unitOfWork.CommitAsync();
        await _unitOfWork.ForumRepo.InvalidateCachedPermissionsAsync(update.ForumId, update.TargetUserId);
    }

    public async Task InvalidateCacheAsync(Guid forumId, Guid userId)
    {
        await _unitOfWork.ForumRepo.InvalidateCachedPermissionsAsync(forumId, userId);
    }

    private ForumPermissionType ResolvePermissions(Domain.Entities.ForumUser forumUser)
    {
        if (forumUser.Role == ForumRole.Admin)
            return ForumPermissionType.All;

        if (forumUser.PermissionOverrides.HasValue)
            return (ForumPermissionType)forumUser.PermissionOverrides.Value;

        return GetDefaultPermissions(forumUser.Role);
    }
}
