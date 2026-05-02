using ForumService.ForumService.Application.Enums;
using ForumService.ForumService.Application.Permissions;
using Xunit;

namespace ForumService.ForumService.Tests.UnitTests;

public sealed class RolePermissionStrategyTests
{
    private static IRolePermissionStrategy[] AllStrategies =>
    [
        new AdminRolePermissionStrategy(),
        new SuperModeratorRolePermissionStrategy(),
        new ModeratorRolePermissionStrategy(),
        new MemberRolePermissionStrategy()
    ];

    [Fact]
    public void Registry_Build_WithAllForumRolesConfigured_ReturnsResolvableStrategies()
    {
        var map = RolePermissionStrategyRegistry.Build(AllStrategies);

        Assert.Equal(4, map.Count);
        foreach (ForumRole role in Enum.GetValues<ForumRole>())
            Assert.True(map.TryGetValue(role, out _), role.ToString());
    }

    [Fact]
    public void Registry_Build_DuplicateRole_ThrowsInvalidOperationException()
    {
        var dup = AllStrategies.Append(new MemberRolePermissionStrategy()).ToArray();

        Assert.Throws<InvalidOperationException>(() => RolePermissionStrategyRegistry.Build(dup));
    }

    [Fact]
    public void Registry_Build_MissingForumRole_ThrowsInvalidOperationException()
    {
        var incomplete = new IRolePermissionStrategy[]
        {
            new AdminRolePermissionStrategy(),
            new SuperModeratorRolePermissionStrategy(),
            new ModeratorRolePermissionStrategy()
        };

        Assert.Throws<InvalidOperationException>(() => RolePermissionStrategyRegistry.Build(incomplete));
    }

    [Fact]
    public void Strategies_GetDefaultPermissions_MatchHistoricalRoleDefaults()
    {
        ForumPermissionType expectedSuperModerator =
            ForumPermissionType.ManageForumInfo | ForumPermissionType.ManageRoles |
            ForumPermissionType.PinThread | ForumPermissionType.LockThread |
            ForumPermissionType.DeleteThread | ForumPermissionType.EditAnyThread |
            ForumPermissionType.DeleteComment | ForumPermissionType.EditAnyComment |
            ForumPermissionType.BanMember | ForumPermissionType.ManageTags |
            ForumPermissionType.CreateThread | ForumPermissionType.CreateComment |
            ForumPermissionType.Vote;

        ForumPermissionType expectedModerator =
            ForumPermissionType.PinThread | ForumPermissionType.LockThread |
            ForumPermissionType.DeleteThread | ForumPermissionType.DeleteComment |
            ForumPermissionType.BanMember | ForumPermissionType.ManageTags |
            ForumPermissionType.CreateThread | ForumPermissionType.CreateComment |
            ForumPermissionType.Vote;

        ForumPermissionType expectedMember =
            ForumPermissionType.CreateThread | ForumPermissionType.CreateComment |
            ForumPermissionType.Vote;

        Assert.Equal(ForumPermissionType.All, new AdminRolePermissionStrategy().GetDefaultPermissions());
        Assert.Equal(expectedSuperModerator, new SuperModeratorRolePermissionStrategy().GetDefaultPermissions());
        Assert.Equal(expectedModerator, new ModeratorRolePermissionStrategy().GetDefaultPermissions());
        Assert.Equal(expectedMember, new MemberRolePermissionStrategy().GetDefaultPermissions());
    }
}
