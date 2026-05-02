using AutoMapper;
using ForumService.ForumService.Domain.Entities;
using ForumService.ForumService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using ForumService.ForumService.Application.DTOs;
using ForumService.ForumService.Application.Enums;
using ForumService.ForumService.Application.Interfaces.Repositories;
using ForumService.ForumService.Application.Requests;
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

        public async Task<List<ForumThread>> GetThreadsByForumIdAsync(ForumThreadPageQuery query, CancellationToken cancellationToken = default)
        {
            var threads = _context.Threads
                .AsNoTracking()
                .Where(t => t.ForumId == query.ForumId);
            
            if (query.SortBy == SortBy.Hot)
            {
                // Commented for testing
                /*threads = threads.Where(t => t.CreatedAt > DateTime.UtcNow - TimeSpan.FromDays(1))
                    .OrderByDescending(t => t.Upvote);*/
            } 
            else if (query.SortBy == SortBy.Top)
            {
                if (query.SortDate == SortDate.Day)
                    threads = threads.Where(t => t.CreatedAt > DateTime.UtcNow - TimeSpan.FromDays(1));
                else if (query.SortDate == SortDate.Month)
                    threads = threads.Where(t => t.CreatedAt > DateTime.UtcNow - TimeSpan.FromDays(30));
                else if (query.SortDate == SortDate.Year)
                    threads = threads.Where(t => t.CreatedAt > DateTime.UtcNow - TimeSpan.FromDays(365));
                threads = threads.OrderByDescending(t => t.Upvote);
            }
            else if (query.SortBy == SortBy.Latest)
                threads = threads.OrderByDescending(t => t.CreatedAt);
            
            threads = threads
                .Skip((query.PageNumber - 1) * query.PageSize)
                .Take(query.PageSize);
            
            var threadList = await threads
                .Include(t => t.PollItems)
                .Include(t => t.Tags)
                .ToListAsync(cancellationToken);
            
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

        public async Task<ForumThread?> GetThreadByIdAsync(Guid threadId, CancellationToken cancellationToken = default)
        {
            var thread = await _context.Threads
                .AsNoTracking()
                .Include(t => t.PollItems)
                .Include(t => t.Votes)
                .Include(t => t.Tags)
                .SingleOrDefaultAsync(t => t.Id == threadId, cancellationToken);
            
            return _mapper.Map<ForumThread>(thread);
        }
        
        public async Task<List<ForumThread>> GetHomeFeedCandidatesAsync(List<Guid>? forumIds, int count, DateTime cutoff, CancellationToken cancellationToken = default)
        {
            var query = _context.Threads
                .AsNoTracking()
                .Include(t => t.PollItems)
                .Include(t => t.Tags)
                .Where(t => t.CreatedAt > cutoff);

            if (forumIds is { Count: > 0 })
                query = query.Where(t => forumIds.Contains(t.ForumId));

            var candidates = await query
                .OrderByDescending(t => t.Upvote)
                .ThenByDescending(t => t.CreatedAt)
                .Take(count)
                .ToListAsync(cancellationToken);

            return _mapper.Map<List<ForumThread>>(candidates);
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
