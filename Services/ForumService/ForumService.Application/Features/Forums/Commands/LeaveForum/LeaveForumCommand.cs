using MediatR;

namespace ForumService.ForumService.Application.Features.Forums.Commands.LeaveForum;

public sealed record LeaveForumCommand(
    Guid ForumId,
    Guid UserId) : IRequest;
