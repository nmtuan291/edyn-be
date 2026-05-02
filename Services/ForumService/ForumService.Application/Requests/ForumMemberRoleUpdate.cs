using ForumService.ForumService.Application.Enums;

namespace ForumService.ForumService.Application.Requests;

public record ForumMemberRoleUpdate(
    Guid ForumId,
    Guid TargetUserId,
    ForumRole NewRole,
    Guid ActingUserId);
