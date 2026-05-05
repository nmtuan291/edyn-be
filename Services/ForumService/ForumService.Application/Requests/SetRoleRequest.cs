using ForumService.ForumService.Application.Enums;

namespace ForumService.ForumService.Application.Requests;

public record SetRoleRequest
{
    public ForumRole Role { get; init; }
}
