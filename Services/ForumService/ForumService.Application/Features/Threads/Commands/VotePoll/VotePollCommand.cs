using ForumService.ForumService.Application.DTOs;
using MediatR;

namespace ForumService.ForumService.Application.Features.Threads.Commands.VotePoll;

public sealed record VotePollCommand(
    Guid UserId,
    Guid ThreadId,
    string PollContent) : IRequest<ForumThreadDto?>;
