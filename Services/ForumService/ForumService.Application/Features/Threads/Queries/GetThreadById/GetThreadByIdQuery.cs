using ForumService.ForumService.Application.DTOs;
using MediatR;

namespace ForumService.ForumService.Application.Features.Threads.Queries.GetThreadById;

public sealed record GetThreadByIdQuery(
    Guid ThreadId,
    string? UserId) : IRequest<ForumThreadDto?>;
