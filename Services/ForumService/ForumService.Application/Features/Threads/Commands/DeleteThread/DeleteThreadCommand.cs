using MediatR;

namespace ForumService.ForumService.Application.Features.Threads.Commands.DeleteThread;

public sealed record DeleteThreadCommand(
    Guid ThreadId,
    Guid UserId) : IRequest;
