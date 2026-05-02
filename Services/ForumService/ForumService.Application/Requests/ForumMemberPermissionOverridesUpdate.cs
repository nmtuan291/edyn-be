using ForumService.ForumService.Application.Enums;

namespace ForumService.ForumService.Application.Requests;

public record ForumMemberPermissionOverridesUpdate(
    Guid ForumId,
    Guid TargetUserId,
    ForumPermissionType Overrides,
    Guid ActingUserId);
