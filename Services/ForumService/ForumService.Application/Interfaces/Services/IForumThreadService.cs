using ForumService.ForumService.Application.DTOs;
using ForumService.ForumService.Application.Requests;

namespace ForumService.ForumService.Application.Interfaces.Services
{
    public interface IForumThreadService
    {
        Task<List<CommentDto>> GetCommentsByThreadId(Guid threadId, string? userId, CancellationToken cancellationToken = default);
        Task<List<ForumThreadDto>> GetThreadsByForumId(ForumThreadListQuery query, CancellationToken cancellationToken = default);
        Task InsertComment(CommentDto comment, Guid userId, string username);
        Task<ForumThreadDto> CreateForumThread(ForumThreadDto forumThread, Guid userId);
        Task<ForumThreadDto?> UpdateThreadVote(Guid threadId, Guid userId, bool isDownVote = false);
        Task<ForumThreadDto?> GetThreadById(Guid threadId, CancellationToken cancellationToken = default);
        Task<CommentDto?> UpdateCommentVote(Guid commentId, Guid userId, bool isDownVote);
    }
}
