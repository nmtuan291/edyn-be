using MediatR;

namespace ForumService.ForumService.Application.Features.Forums.Commands.JoinForum;

public sealed record JoinForumCommand(
    Guid ForumId,
    Guid UserId) : IRequest;
