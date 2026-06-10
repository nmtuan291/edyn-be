using MediatR;

namespace ForumService.ForumService.Application.Features.Forums.Commands.RemoveMember;

public sealed record RemoveMemberCommand(
    Guid ForumId,
    Guid TargetUserId,
    Guid ActingUserId) : IRequest;
