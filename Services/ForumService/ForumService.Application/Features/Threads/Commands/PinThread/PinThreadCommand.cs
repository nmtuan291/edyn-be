using ForumService.ForumService.Application.DTOs;
using MediatR;

namespace ForumService.ForumService.Application.Features.Threads.Commands.PinThread;

public sealed record PinThreadCommand(
    Guid ThreadId,
    Guid UserId,
    bool IsPinned) : IRequest<ForumThreadDto?>;
