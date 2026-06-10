using ForumService.ForumService.Application.DTOs;
using ForumService.ForumService.Application.Enums;
using MediatR;

namespace ForumService.ForumService.Application.Features.Threads.Queries.GetForumThreads;

public sealed record GetForumThreadsQuery(
    Guid ForumId,
    string? UserId,
    int PageNumber,
    int PageSize,
    SortBy SortBy,
    SortDate SortDate) : IRequest<PagedResult<ForumThreadDto>>;
