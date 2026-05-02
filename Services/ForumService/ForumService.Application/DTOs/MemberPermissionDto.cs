using ForumService.ForumService.Application.Enums;

namespace ForumService.ForumService.Application.DTOs;

public class MemberPermissionDto
{
    public Guid UserId { get; set; }
    public Guid ForumId { get; set; }
    public ForumRole Role { get; set; }
    public ForumPermissionType EffectivePermissions { get; set; }
    public ForumPermissionType? PermissionOverrides { get; set; }
}
