using ForumService.ForumService.Domain.Entities;

namespace ForumService.ForumService.Application.Interfaces.Repositories;

public interface ICommentQueryRepository
{
    Task<List<Comment>> GetCommentByThreadIdAsync(Guid threadId, CancellationToken cancellationToken = default);
}
