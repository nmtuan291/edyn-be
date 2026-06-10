using ForumService.ForumService.Application.DTOs;
using MediatR;

namespace ForumService.ForumService.Application.Features.Feeds.Queries.GetHomeFeed;

public sealed record GetHomeFeedQuery(
    string? UserId,
    int Page,
    int PageSize) : IRequest<List<ForumThreadDto>>;
