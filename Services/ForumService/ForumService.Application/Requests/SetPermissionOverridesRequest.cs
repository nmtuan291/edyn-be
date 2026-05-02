using ForumService.ForumService.Application.Enums;

namespace ForumService.ForumService.Application.Requests;

public class SetPermissionOverridesRequest
{
    public ForumPermissionType Permissions { get; set; }
}
