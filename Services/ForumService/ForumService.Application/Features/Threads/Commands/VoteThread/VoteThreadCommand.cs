using ForumService.ForumService.Application.DTOs;
using MediatR;

namespace ForumService.ForumService.Application.Features.Threads.Commands.VoteThread;

public sealed record VoteThreadCommand(
    Guid ThreadId,
    Guid UserId,
    bool IsDownVote) : IRequest<VoteThreadResult>;

public sealed record VoteThreadResult(
    ForumThreadDto? Thread,
    bool Forbidden = false);
