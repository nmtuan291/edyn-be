using ForumService.ForumService.Application.Enums;

namespace ForumService.ForumService.Application.DTOs;

public record MemberPermissionDto
{
    public Guid UserId { get; init; }
    public Guid ForumId { get; init; }
    public ForumRole Role { get; init; }
    public ForumPermissionType EffectivePermissions { get; init; }
    public ForumPermissionType? PermissionOverrides { get; init; }
}
