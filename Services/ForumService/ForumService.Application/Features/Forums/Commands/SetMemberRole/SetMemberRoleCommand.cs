using ForumService.ForumService.Application.Enums;
using MediatR;

namespace ForumService.ForumService.Application.Features.Forums.Commands.SetMemberRole;

public sealed record SetMemberRoleCommand(
    Guid ForumId,
    Guid TargetUserId,
    ForumRole Role,
    Guid ActingUserId) : IRequest;
