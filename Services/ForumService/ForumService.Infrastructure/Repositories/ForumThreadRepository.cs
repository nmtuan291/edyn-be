using ForumService.ForumService.Domain.Entities;
using ForumService.ForumService.Infrastructure.Data;
using ForumService.ForumService.Application.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;

namespace ForumService.ForumService.Infrastructure.Repositories
{
    public class ForumThreadRepository: IForumThreadRepository
    {
        private readonly ForumDbContext _context;

        public ForumThreadRepository(ForumDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<ForumThread>> GetThreadsByForumIdAsync(Guid forumId)
        {
            var threads = await _context.Threads
                    .Where(t => t.ForumId == forumId)
                    .ToListAsync();

            return threads;
        }

        public async Task<IEnumerable<Comment>> GetCommentByThreadIdAsync(Guid threadId)
        {
            var comments = await _context.Comments
                .Where(t => t.ThreadId == threadId)
                .ToListAsync();

            return comments;
        }

        public async Task AddCommentAsync()
        {

        }
    }
}
