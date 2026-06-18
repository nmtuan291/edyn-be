using ForumService.ForumService.Application.DTOs;
using MediatR;

namespace ForumService.ForumService.Application.Features.Threads.Queries.SearchThreads;

public sealed record SearchThreadsQuery(
    string Query,
    int Page,
    int PageSize) : IRequest<PagedResult<ForumThreadDto>>;
