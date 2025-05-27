using ForumService.ForumService.Domain.Entities;

namespace ForumService.ForumService.Application.Interfaces.Repositories
{
    public interface IForumThreadRepository
    {
        public Task<IEnumerable<ForumThread>> GetThreadsByForumIdAsync(Guid forumId);
        public Task<IEnumerable<Comment>> GetCommentByThreadIdAsync(Guid threadId);
        public Task AddCommentAsync();
    }
}
