using ForumService.ForumService.Application.Enums;

namespace ForumService.ForumService.Application.Permissions;

public static class RolePermissionStrategyRegistry
{
    public static IReadOnlyDictionary<ForumRole, IRolePermissionStrategy> Build(
        IEnumerable<IRolePermissionStrategy> strategies)
    {
        var byRole = new Dictionary<ForumRole, IRolePermissionStrategy>();
        foreach (var strategy in strategies)
        {
            if (!byRole.TryAdd(strategy.Role, strategy))
                throw new InvalidOperationException(
                    $"Duplicate IRolePermissionStrategy registered for ForumRole.{strategy.Role}.");
        }

        foreach (ForumRole role in Enum.GetValues<ForumRole>())
        {
            if (!byRole.ContainsKey(role))
                throw new InvalidOperationException(
                    $"No IRolePermissionStrategy registered for ForumRole.{role}.");
        }

        return byRole;
    }
}
