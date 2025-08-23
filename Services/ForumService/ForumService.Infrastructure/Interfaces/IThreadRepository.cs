using ForumService.ForumService.Application.DTOs;
using ForumService.ForumService.Application.Enums;
using ForumService.ForumService.Domain.Entities;

namespace ForumService.ForumService.Infrastructure.Interfaces
{
    public interface IThreadRepository
    {
        Task<List<ForumThread>> GetThreadsByForumIdAsync(Guid forumId, SortBy sortBy,
            SortDate sortDate, int pageNumber, int pageSize);
        Task DeleteThreadByIdAsync(Guid threadId);
        Task InsertThreadAsync(ForumThread thread);
        void UpdateThread(ForumThread thread);
        Task<ForumThread?> GetThreadByIdAsync(Guid threadId);
    }
}
