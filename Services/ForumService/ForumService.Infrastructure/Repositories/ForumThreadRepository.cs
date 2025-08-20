using ForumService.ForumService.Domain.Entities;
using ForumService.ForumService.Infrastructure.Data;
using ForumService.ForumService.Application.Interfaces.Repositories;
using Microsoft.EntityFrameworkCore;
using ForumService.ForumService.Application.DTOs;
using ForumService.ForumService.Application.Enums;
using StackExchange.Redis;

namespace ForumService.ForumService.Infrastructure.Repositories
{
    public class ForumThreadRepository: IForumThreadRepository
    {
        private readonly ForumDbContext _context;
        private readonly IDatabase _redis;

        public ForumThreadRepository(ForumDbContext context, IConnectionMultiplexer redis)
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

        public async Task<List<Comment>> GetCommentByThreadIdAsync(Guid threadId)
        {
            var  allComments = await _context.Comments
                .Where(t => t.ThreadId == threadId)
                .ToListAsync();

            var commentDict = allComments.ToDictionary(c => c.Id);
            foreach (var comment in allComments)
            {
                if (comment.ParentId != null && commentDict.TryGetValue(comment.ParentId.Value, out var parentComment))
                {
                    if (parentComment.ChildrenComments == null) 
                        parentComment.ChildrenComments = new List<Comment>();
                    
                    parentComment.ChildrenComments.Add(comment);
                }
            }
            
            return allComments.Where(comment => comment.ParentId == null).ToList();
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
                _context.Threads.Remove(thread);
        }

        public async Task DeleteCommentById(Guid commentId)
        {
            var comment = await _context.Comments.FindAsync(commentId);
            if (comment != null)
                _context.Comments.Remove(comment);
        }

        public async Task<Comment?> GetParentCommentAsync(Guid commentId)
        {
            return await _context.Comments
                .Where(c => c.Id == commentId)
                .SingleOrDefaultAsync();
        }

        public async Task<ForumThread?> GetThreadByIdAsync(Guid threadId)
        {
            return await _context.Threads
                .Include(t => t.PollItems)
                .SingleOrDefaultAsync(t => t.Id == threadId);
        }
        
        // Vote
        public void UpdateThread(ForumThread thread)
        {
            _context.Threads.Update(thread);
        }

        public async Task AddThreadVoteAsync(ThreadVote vote, Guid forumId)
        {
            await _context.ThreadVotes.AddAsync(vote);
            await _redis.HashSetAsync($"vote:{vote.UserId}:{forumId}", 
                vote.ThreadId.ToString(), vote.DownVote);
        }

        public async Task DeleteThreadVote(ThreadVote vote, Guid forumId)
        {
            _context.ThreadVotes.Remove(vote);
            await _redis.HashDeleteAsync($"vote:{vote.UserId}:{forumId}", vote.ThreadId.ToString());
        }

        public async Task UpdateThreadVote(ThreadVote vote, Guid forumId)
        {
            _context.ThreadVotes.Update(vote);
            await _redis.HashSetAsync($"vote:{vote.UserId}:{forumId}", 
                vote.ThreadId.ToString(), vote.DownVote);
        }

        public async Task<ThreadVote?> GetThreadVoteAsync(Guid threadId, Guid userId)
        {
            return await _context.ThreadVotes
                .SingleOrDefaultAsync(t => t.ThreadId == threadId && t.UserId == userId);
        }

        public async Task<int> CountUpvotesAsync(Guid threadId)
        {
            return await _context.ThreadVotes
                .CountAsync(t => t.ThreadId == threadId && t.DownVote == false);
        }

        public async Task<Dictionary<Guid, bool>> GetVotedThreadsAsync(Guid userId, Guid forumId)
        {
            var result = await _redis.HashGetAllAsync($"vote:{userId}:{forumId}");
            return result.ToDictionary(r => Guid.Parse(r.Name.ToString()), r => (bool)r.Value);
        }
    }
}
