using AutoMapper;
using ForumService.ForumService.Domain.Entities;
using ForumService.ForumService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using ForumService.ForumService.Application.DTOs;
using ForumService.ForumService.Application.Enums;
using ForumService.ForumService.Application.Interfaces.Repositories;
using ForumService.ForumService.Application.Requests;
using ForumService.ForumService.Infrastructure.Extensions;
using ForumService.ForumService.Infrastructure.Messaging;
using ForumService.ForumService.Infrastructure.Models;
using StackExchange.Redis;

namespace ForumService.ForumService.Infrastructure.Repositories
{
    public class ThreadRepository: IThreadRepository
    {
        private readonly ForumDbContext _context;
        private readonly IDatabase _redis;
        private readonly IMapper _mapper;
        private readonly BoundedChannelBuffer<TelemetryLog> _buffer;
        private readonly IHttpContextAccessor _httpContextAccessor;
        
        public ThreadRepository(ForumDbContext context, IConnectionMultiplexer redis, IMapper mapper, 
            BoundedChannelBuffer<TelemetryLog> buffer, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _redis = redis.GetDatabase();
            _mapper = mapper;
            _buffer = buffer;
            _httpContextAccessor = httpContextAccessor;
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
                .Include(t => t.PollVotes)
                .Include(t => t.Tags)
                .ToListAsync(cancellationToken);

            /*var telemetryLog = new TelemetryLog();

            _buffer.Writer.TryWrite(telemetryLog);*/
            
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

        public async Task<ForumThread?> GetThreadByIdAsync(Guid threadId, Guid userId = default, CancellationToken cancellationToken = default)
        {
            var thread = await _context.Threads
                .AsNoTracking()
                .Include(t => t.PollItems)
                .Include(t => t.PollVotes)
                .Include(t => t.Votes)
                .Include(t => t.Tags)
                .Include(t => t.ForumEf)
                .SingleOrDefaultAsync(t => t.Id == threadId, cancellationToken);

            if (thread?.ForumEf != null && userId != Guid.Empty)
                await _redis.WriteVisitForum(userId, thread.ForumEf);
            
            return _mapper.Map<ForumThread>(thread);
        }
        
        public async Task<List<ForumThread>> GetHomeFeedCandidatesAsync(List<Guid>? forumIds, int count, DateTime cutoff, CancellationToken cancellationToken = default)
        {
            var query = _context.Threads
                .AsNoTracking()
                .Include(t => t.PollItems)
                .Include(t => t.PollVotes)
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
                .Include(t => t.PollVotes)
                .Include(t => t.Votes)
                .First(t => t.Id == thread.Id);

            _mapper.Map(thread, ef);
        }

        public async Task<int> GetThreadCountByForumIdAsync(Guid forumId, CancellationToken cancellationToken = default)
        {
            return await _context.Threads
                .AsNoTracking()
                .CountAsync(t => t.ForumId == forumId, cancellationToken);
        }

        public async Task<List<ForumThread>> SearchThreadsAsync(string query, int page, int pageSize, CancellationToken cancellationToken = default)
        {
            var lowerQuery = query.ToLower();
            var threads = await _context.Threads
                .AsNoTracking()
                .Include(t => t.PollItems)
                .Include(t => t.PollVotes)
                .Include(t => t.Tags)
                .Where(t => t.Title.ToLower().Contains(lowerQuery) || t.Content.ToLower().Contains(lowerQuery))
                .OrderByDescending(t => t.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);
            return _mapper.Map<List<ForumThread>>(threads);
        }

        public async Task<int> SearchThreadsCountAsync(string query, CancellationToken cancellationToken = default)
        {
            var lowerQuery = query.ToLower();
            return await _context.Threads
                .AsNoTracking()
                .CountAsync(t => t.Title.ToLower().Contains(lowerQuery) || t.Content.ToLower().Contains(lowerQuery), cancellationToken);
        }

        public async Task<ForumThread?> VotePollAsync(Guid userId, Guid threadId, string pollContent)
        {
            // Load WITH tracking so EF properly detects additions/removals
            var thread = await _context.Threads
                .Include(t => t.PollItems)
                .Include(t => t.PollVotes)
                .Include(t => t.Tags)
                .FirstOrDefaultAsync(t => t.Id == threadId);

            if (thread == null) return null;

            var pollItem = thread.PollItems?.FirstOrDefault(p => p.PollContent == pollContent);
            if (pollItem == null) return null;

            var existingVote = thread.PollVotes?.FirstOrDefault(v => v.UserId == userId);

            if (existingVote != null)
            {
                if (existingVote.PollContent == pollContent)
                {
                    // Toggle off: user clicked the same option again
                    _context.PollVotes.Remove(existingVote);
                    pollItem.VoteCount = Math.Max(0, pollItem.VoteCount - 1);
                }
                else
                {
                    // Switch: user picked a different option
                    var oldPollItem = thread.PollItems?.FirstOrDefault(p => p.PollContent == existingVote.PollContent);
                    if (oldPollItem != null) oldPollItem.VoteCount = Math.Max(0, oldPollItem.VoteCount - 1);

                    existingVote.PollContent = pollContent;
                    pollItem.VoteCount++;
                }
            }
            else
            {
                // New vote
                _context.PollVotes.Add(new PollVoteEf
                {
                    UserId = userId,
                    ThreadId = threadId,
                    PollContent = pollContent
                });
                pollItem.VoteCount++;
            }

            return _mapper.Map<ForumThread>(thread);
        }
    }
}
