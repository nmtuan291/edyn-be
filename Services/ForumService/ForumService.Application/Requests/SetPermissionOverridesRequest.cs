using ForumService.ForumService.Application.Enums;

namespace ForumService.ForumService.Application.Requests;

public record SetPermissionOverridesRequest
{
    public ForumPermissionType Permissions { get; init; }
}
