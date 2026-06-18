using ForumService.ForumService.Application.DTOs;
using ForumService.ForumService.Application.Enums;
using MediatR;

namespace ForumService.ForumService.Application.Features.Feeds.Queries.GetHomeFeed;

public sealed record GetHomeFeedQuery(
    string? UserId,
    int Page,
    int PageSize,
    SortBy Sort = SortBy.Hot,
    SortDate Date = SortDate.All) : IRequest<List<ForumThreadDto>>;
