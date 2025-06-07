using ForumService.ForumService.Domain.Entities;
using ForumService.ForumService.Infrastructure.Data;
using ForumService.ForumService.Application.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;
using ForumService.ForumService.Application.DTOs;

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

        public async Task InsertCommentAsync(Comment comment)
        {
            await _context.Comments.AddAsync(comment);
        }

        public async Task InsertThreadAsync(ForumThread thread)
        {
            await _context.Threads.AddAsync(thread);
        }

        public async Task DeleteThreadByIdAsync(Guid threadId)
        {
            var thread = await _context.Threads.FindAsync(threadId);
            if (thread != null)
            {
                _context.Threads.Remove(thread);
            }
        }

        public async Task DeleteCommentById(Guid commentId)
        {
            var comment = await _context.Comments.FindAsync(commentId);
            if (comment != null)
            {
                _context.Comments.Remove(comment);
            }
        }
    }
}
