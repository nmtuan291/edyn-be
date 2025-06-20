using ForumService.ForumService.Application.DTOs;
using ForumService.ForumService.Application.Enums;

namespace ForumService.ForumService.Application.Interfaces.Services
{
    public interface IForumThreadService
    {
        Task<IEnumerable<CommentDto>> GetCommentsByThreadId(Guid threadId);

        Task<IEnumerable<ForumThreadDto>> GetThreadsByForumId(Guid forumId, int pageNumber, int pageSize,
            SortBy sortBy = SortBy.Hot, SortDate sortDate = SortDate.All);
        Task InsertComment(CommentDto comment);
    }
}
