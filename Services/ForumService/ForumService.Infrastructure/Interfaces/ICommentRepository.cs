using ForumService.ForumService.Domain.Entities;

namespace ForumService.ForumService.Infrastructure.Interfaces;

public interface ICommentRepository
{
    Task<List<Comment>> GetCommentByThreadIdAsync(Guid threadId);
    Task InsertCommentAsync(Comment comment);
    Task DeleteCommentById(Guid commentId);
    Task<Comment?> GetParentCommentAsync(Guid commentId);
}