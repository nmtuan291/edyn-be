using ForumService.ForumService.Application.Enums;

namespace ForumService.ForumService.Application.Requests;

public record ForumThreadListQuery(
    Guid ForumId,
    string? UserId,
    int PageNumber,
    int PageSize,
    SortBy SortBy = SortBy.Hot,
    SortDate SortDate = SortDate.All);
