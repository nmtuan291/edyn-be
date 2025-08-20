using ForumService.ForumService.Application.DTOs;
using ForumService.ForumService.Application.Enums;
using ForumService.ForumService.Domain.Entities;

namespace ForumService.ForumService.Application.Interfaces.Repositories
{
    public interface IForumThreadRepository
    {
        Task<List<ForumThread>> GetThreadsByForumIdAsync(Guid forumId, SortBy sortBy,
            SortDate sortDate, int pageNumber, int pageSize);
        Task<List<Comment>> GetCommentByThreadIdAsync(Guid threadId);
        Task InsertCommentAsync(Comment comment);
        Task DeleteCommentById(Guid commentId);
        Task DeleteThreadByIdAsync(Guid threadId);
        Task InsertThreadAsync(ForumThread thread);
        Task<ForumThread?> GetThreadByIdAsync(Guid threadId);
        Task<Comment?> GetParentCommentAsync(Guid commentId);
        void UpdateThread(ForumThread thread);
        Task AddThreadVoteAsync(ThreadVote vote, Guid forumId);
        Task DeleteThreadVote(ThreadVote vote, Guid forumId);
        Task<ThreadVote?> GetThreadVoteAsync(Guid threadId, Guid userId);
        Task UpdateThreadVote(ThreadVote vote, Guid forumId);
        Task<int> CountUpvotesAsync(Guid threadId);
        Task<Dictionary<Guid, bool>> GetVotedThreadsAsync(Guid userId, Guid forumId);
    }
}
