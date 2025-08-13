using ForumService.ForumService.Application.DTOs;
using ForumService.ForumService.Application.Enums;

namespace ForumService.ForumService.Application.Interfaces.Services
{
    public interface IForumThreadService
    {
        Task<List<CommentDto>> GetCommentsByThreadId(Guid threadId);

        Task<List<ForumThreadDto>> GetThreadsByForumId(Guid forumId, int pageNumber, int pageSize,
            SortBy sortBy = SortBy.Hot, SortDate sortDate = SortDate.All);
        Task InsertComment(CommentDto comment, Guid userId, string username);
        Task CreateForumThread(ForumThreadDto forumThread, Guid userId);
        Task<ForumThreadDto?> GetThreadById(Guid threadId);
    }
}
