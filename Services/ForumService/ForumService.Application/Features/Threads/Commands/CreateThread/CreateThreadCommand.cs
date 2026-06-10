using ForumService.ForumService.Application.DTOs;
using MediatR;

namespace ForumService.ForumService.Application.Features.Threads.Commands.CreateThread;

public sealed record CreateThreadCommand(
    ForumThreadDto Thread,
    Guid UserId) : IRequest<CreateThreadResult>;

public sealed record CreateThreadResult(
    ForumThreadDto? Thread,
    bool Forbidden = false);
