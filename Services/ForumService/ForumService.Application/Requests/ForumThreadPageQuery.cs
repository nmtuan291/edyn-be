using ForumService.ForumService.Application.Enums;

namespace ForumService.ForumService.Application.Requests;

public record ForumThreadPageQuery(
    Guid ForumId,
    SortBy SortBy,
    SortDate SortDate,
    int PageNumber,
    int PageSize);
