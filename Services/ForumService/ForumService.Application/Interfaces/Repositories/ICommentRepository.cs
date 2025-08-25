using ForumService.ForumService.Domain.Entities;

namespace ForumService.ForumService.Application.Interfaces.Repositories;

public interface ICommentRepository
{
    Task<List<Comment>> GetCommentByThreadIdAsync(Guid threadId);
    Task InsertCommentAsync(Comment comment);
    Task DeleteCommentById(Guid commentId);
    Task<Comment?> GetParentCommentAsync(Guid commentId);
}