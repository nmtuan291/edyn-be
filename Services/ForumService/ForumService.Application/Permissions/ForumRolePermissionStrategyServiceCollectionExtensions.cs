using Microsoft.Extensions.DependencyInjection;

namespace ForumService.ForumService.Application.Permissions;

public static class ForumRolePermissionStrategyServiceCollectionExtensions
{
    public static IServiceCollection AddForumRolePermissionStrategies(this IServiceCollection services)
    {
        services.AddSingleton<IRolePermissionStrategy, AdminRolePermissionStrategy>();
        services.AddSingleton<IRolePermissionStrategy, SuperModeratorRolePermissionStrategy>();
        services.AddSingleton<IRolePermissionStrategy, ModeratorRolePermissionStrategy>();
        services.AddSingleton<IRolePermissionStrategy, MemberRolePermissionStrategy>();
        return services;
    }
}
