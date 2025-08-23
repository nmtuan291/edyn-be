using ForumService.ForumService.Domain.Entities;
using ForumService.ForumService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using ForumService.ForumService.Application.DTOs;
using ForumService.ForumService.Application.Enums;
using ForumService.ForumService.Infrastructure.Interfaces;
using StackExchange.Redis;

namespace ForumService.ForumService.Infrastructure.Repositories
{
    public class ThreadRepository: IThreadRepository
    {
        private readonly ForumDbContext _context;
        private readonly IDatabase _redis;

        public ThreadRepository(ForumDbContext context, IConnectionMultiplexer redis)
        {
            _context = context;
            _redis = redis.GetDatabase();
        }

        public async Task<List<ForumThread>> GetThreadsByForumIdAsync(Guid forumId, SortBy sortBy, 
            SortDate sortDate, int pageNumber, int pageSize)
        {
            var threads = _context.Threads
                .Where(t => t.ForumId == forumId);
            
            if (sortBy == SortBy.Hot)
            {
                // Commented for testing
                /*threads = threads.Where(t => t.CreatedAt > DateTime.UtcNow - TimeSpan.FromDays(1))
                    .OrderByDescending(t => t.Upvote);*/
            } 
            else if (sortBy == SortBy.Top)
            {
                if (sortDate == SortDate.Day)
                    threads = threads.Where(t => t.CreatedAt > DateTime.UtcNow - TimeSpan.FromDays(1));
                else if (sortDate == SortDate.Month)
                    threads = threads.Where(t => t.CreatedAt > DateTime.UtcNow - TimeSpan.FromDays(30));
                else if (sortDate == SortDate.Year)
                    threads = threads.Where(t => t.CreatedAt > DateTime.UtcNow - TimeSpan.FromDays(365));
                threads = threads.OrderByDescending(t => t.Upvote);
            }
            else if (sortBy == SortBy.Latest)
                threads = threads.OrderByDescending(t => t.CreatedAt);
            
            
            threads = threads
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize);
            
            
            return await threads
                .Include(t => t.PollItems)
                .ToListAsync();
        }

        public async Task InsertThreadAsync(ForumThread thread)
        {
            await _context.Threads.AddAsync(thread);
        }

        public async Task DeleteThreadByIdAsync(Guid threadId)
        {
            var thread = await _context.Threads.FindAsync(threadId);
            if (thread != null)
                _context.Threads.Remove(thread);
        }

        public async Task<ForumThread?> GetThreadByIdAsync(Guid threadId)
        {
            return await _context.Threads
                .Include(t => t.PollItems)
                .Include(t => t.Votes)
                .SingleOrDefaultAsync(t => t.Id == threadId);
        }
        
        public void UpdateThread(ForumThread thread)
        {
            _context.Threads.Update(thread);
        }
    }
}
