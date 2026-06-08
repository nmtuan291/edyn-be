using ForumService.ForumService.Domain.Entities;

namespace ForumService.ForumService.Application.Interfaces.Repositories;

public interface ICommentRepository
{
    Task<List<Comment>> GetCommentByThreadIdAsync(Guid threadId, CancellationToken cancellationToken = default);
    Task InsertCommentAsync(Comment comment);
    Task DeleteCommentById(Guid commentId);
    Task<Comment?> GetParentCommentAsync(Guid commentId, CancellationToken cancellationToken = default);
    Task<Comment?> GetCommentByIdAsync(Guid commentId, CancellationToken cancellationToken = default);
    Task UpdateCommentAsync(Comment comment);
    Task<(List<(Comment Comment, string ThreadTitle, string RealmShortName)> Comments, int TotalCount)> GetCommentsByUserIdAsync(Guid userId, int page, int pageSize, CancellationToken cancellationToken = default);
}