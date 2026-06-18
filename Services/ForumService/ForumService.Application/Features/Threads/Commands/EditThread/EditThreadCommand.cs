using ForumService.ForumService.Application.DTOs;
using ForumService.ForumService.Application.Requests;
using MediatR;

namespace ForumService.ForumService.Application.Features.Threads.Commands.EditThread;

public sealed record EditThreadCommand(
    Guid ThreadId,
    Guid UserId,
    EditThreadRequest Request) : IRequest<ForumThreadDto?>;
