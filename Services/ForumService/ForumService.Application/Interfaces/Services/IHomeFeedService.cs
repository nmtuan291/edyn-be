using ForumService.ForumService.Application.DTOs;

namespace ForumService.ForumService.Application.Interfaces.Services;

public interface IHomeFeedService
{
    Task<List<ForumThreadDto>> GetHomeFeed(string? userId, int page, int pageSize, CancellationToken cancellationToken = default);
}
