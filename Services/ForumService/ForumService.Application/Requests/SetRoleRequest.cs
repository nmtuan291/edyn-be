using ForumService.ForumService.Application.Enums;

namespace ForumService.ForumService.Application.Requests;

public class SetRoleRequest
{
    public ForumRole Role { get; set; }
}
