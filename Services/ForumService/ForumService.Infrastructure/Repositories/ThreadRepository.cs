using AutoMapper;
using ForumService.ForumService.Domain.Entities;
using ForumService.ForumService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using ForumService.ForumService.Application.DTOs;
using ForumService.ForumService.Application.Enums;
using ForumService.ForumService.Application.Interfaces.Repositories;
using ForumService.ForumService.Infrastructure.Models;
using StackExchange.Redis;

namespace ForumService.ForumService.Infrastructure.Repositories
{
    public class ThreadRepository: IThreadRepository
    {
        private readonly ForumDbContext _context;
        private readonly IDatabase _redis;
        private readonly IMapper _mapper;

        public ThreadRepository(ForumDbContext context, IConnectionMultiplexer redis, IMapper mapper)
        {
            _context = context;
            _redis = redis.GetDatabase();
            _mapper = mapper;
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
            
            
            var threadList = await threads
                .Include(t => t.PollItems)
                .ToListAsync();
            
            return _mapper.Map<List<ForumThread>>(threadList);
        }

        public async Task InsertThreadAsync(ForumThread thread)
        {
            ForumThreadEf forumThreadEf = _mapper.Map<ForumThreadEf>(thread);
            await _context.Threads.AddAsync(forumThreadEf);
        }

        public async Task DeleteThreadByIdAsync(Guid threadId)
        {
            var thread = await _context.Threads.FindAsync(threadId);
            if (thread != null)
                _context.Threads.Remove(thread);
        }

        public async Task<ForumThread?> GetThreadByIdAsync(Guid threadId)
        {
            var thread = await _context.Threads
                .Include(t => t.PollItems)
                .Include(t => t.Votes)
                .SingleOrDefaultAsync(t => t.Id == threadId);
            
            return _mapper.Map<ForumThread>(thread);
        }
        
        public void UpdateThread(ForumThread thread)
        {
            var ef = _context.Threads
                .Include(t => t.PollItems)
                .Include(t => t.Votes)
                .First(t => t.Id == thread.Id);

            _mapper.Map(thread, ef);
        }
    }
}
