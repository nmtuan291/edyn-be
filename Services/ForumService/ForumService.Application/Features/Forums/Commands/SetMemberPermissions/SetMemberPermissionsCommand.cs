using ForumService.ForumService.Application.Enums;
using MediatR;

namespace ForumService.ForumService.Application.Features.Forums.Commands.SetMemberPermissions;

public sealed record SetMemberPermissionsCommand(
    Guid ForumId,
    Guid TargetUserId,
    ForumPermissionType Permissions,
    Guid ActingUserId) : IRequest;
