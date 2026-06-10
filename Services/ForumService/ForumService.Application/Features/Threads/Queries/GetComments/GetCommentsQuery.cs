using ForumService.ForumService.Application.DTOs;
using MediatR;

namespace ForumService.ForumService.Application.Features.Threads.Queries.GetComments;

public sealed record GetCommentsQuery(
    Guid ThreadId,
    string? UserId) : IRequest<List<CommentDto>>;
