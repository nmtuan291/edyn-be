using ForumService.ForumService.Application.DTOs;

namespace ForumService.ForumService.Application.Interfaces.Services
{
    public interface IForumThreadService
    {
        Task<IEnumerable<CommentDto>> GetCommentsByThreadId(Guid threadId);
        Task<IEnumerable<ForumThreadDto>> GetThreadsByForumId(Guid forumId);
        Task InsertComment(CommentDto comment);
    }
}
