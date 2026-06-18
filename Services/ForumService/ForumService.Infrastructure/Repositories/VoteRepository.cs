using AutoMapper;
using ForumService.ForumService.Application.Interfaces.Repositories;
using ForumService.ForumService.Application.Requests;
using ForumService.ForumService.Domain.Entities;
using ForumService.ForumService.Infrastructure.Data;
using ForumService.ForumService.Infrastructure.Models;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;

namespace ForumService.ForumService.Infrastructure.Repositories;

public class VoteRepository : IVoteRepository, IVoteQueryRepository
{
    private readonly ForumDbContext _context;
    private readonly IDatabase _redis;
    private readonly IMapper _mapper;

    public VoteRepository(ForumDbContext dbContext, IConnectionMultiplexer redis, IMapper mapper)
    {
        _context = dbContext;
        _redis = redis.GetDatabase();
        _mapper = mapper;
    }
    
    public async Task AddThreadVoteAsync(ThreadVote vote, Guid forumId)
    {
        ThreadVoteEf threadVoteEf = _mapper.Map<ThreadVoteEf>(vote); 
        await _context.ThreadVotes.AddAsync(threadVoteEf);
        await _redis.HashSetAsync($"vote:{vote.UserId}:{forumId}", 
            vote.ThreadId.ToString(), vote.DownVote);
    }
    
    public async Task AddCommentVoteAsync(CommentVote vote, Guid threadId)
    {
        CommentVoteEf commentVoteEf = _mapper.Map<CommentVoteEf>(vote); 
        await _context.CommentVotes.AddAsync(commentVoteEf);
        await _redis.HashSetAsync($"vote:{vote.UserId}:{threadId}", 
            vote.CommentId.ToString(), vote.DownVote);
    }

    public async Task DeleteThreadVote(ThreadVote vote, Guid forumId)
    {
        ThreadVoteEf threadVoteEf = _mapper.Map<ThreadVoteEf>(vote);
        _context.ThreadVotes.Remove(threadVoteEf);
        await _redis.HashDeleteAsync($"vote:{vote.UserId}:{forumId}", vote.ThreadId.ToString());
    }
    
    public async Task UpdateThreadVoteRedisAsync(ThreadVoteRedisUpdate update)
    {
        /*ThreadVoteEf threadVoteEf = _mapper.Map<ThreadVoteEf>(vote);
        _context.ThreadVotes.Update(threadVoteEf);*/
        await _redis.HashSetAsync($"vote:{update.UserId}:{update.ForumId}", 
            update.ThreadId.ToString(), update.IsDownVote);
    }
    
    public async Task RemoveThreadVoteRedisAsync(Guid userId, Guid threadId, Guid forumId)
    {
        await _redis.HashDeleteAsync($"vote:{userId}:{forumId}", 
            threadId.ToString());
    }

    public async Task UpdateCommentVoteRedisAsync(CommentVoteRedisUpdate update)
    {
        /*ThreadVoteEf threadVoteEf = _mapper.Map<ThreadVoteEf>(vote);
        _context.ThreadVotes.Update(threadVoteEf);*/
        await _redis.HashSetAsync($"vote:{update.UserId}:{update.ThreadId}", 
            update.CommentId.ToString(), update.IsDownVote);
    }
    
    public async Task RemoveCommentVoteRedisAsync(Guid userId, Guid commentId, Guid threadId)
    {
        await _redis.HashDeleteAsync($"vote:{userId}:{threadId}", 
            commentId.ToString());
    }

    public async Task<ThreadVote?> GetThreadVoteAsync(Guid threadId, Guid userId, CancellationToken cancellationToken = default)
    {
        var threadVote = await _context.ThreadVotes
            .AsNoTracking()
            .SingleOrDefaultAsync(t => t.ThreadId == threadId && t.UserId == userId, cancellationToken);
        
        return _mapper.Map<ThreadVote>(threadVote);
    }
    
    public async Task<CommentVote?> GetCommentVoteAsync(Guid commentId, Guid userId, CancellationToken cancellationToken = default)
    {
        var commentVote = await _context.CommentVotes
            .AsNoTracking()
            .SingleOrDefaultAsync(t => t.CommentId == commentId && t.UserId == userId, cancellationToken);
        
        return _mapper.Map<CommentVote>(commentVote);
    }

    public async Task<int> CountUpvotesAsync(Guid threadId, CancellationToken cancellationToken = default)
    {
        return await _context.ThreadVotes
            .CountAsync(t => t.ThreadId == threadId && t.DownVote == false, cancellationToken);
    }

    public async Task<Dictionary<Guid, bool>> GetVotedThreadsAsync(Guid userId, Guid forumId)
    {
        var result = await _redis.HashGetAllAsync($"vote:{userId}:{forumId}");
        return result.ToDictionary(r => Guid.Parse(r.Name.ToString()), r => (bool)r.Value);
    }
    
    public async Task<Dictionary<Guid, bool>> GetVotedCommentsAsync(Guid userId, Guid threadId)
    {
        var result = await _redis.HashGetAllAsync($"vote:{userId}:{threadId}");
        return result.ToDictionary(r => Guid.Parse(r.Name.ToString()), r => (bool)r.Value);
    }

    public async Task IncrementTagAffinityAsync(Guid userId, IEnumerable<string> tagNames)
    {
        var key = $"tagaffinity:{userId}";
        foreach (var tag in tagNames)
            await _redis.SortedSetIncrementAsync(key, tag, 1);
    }

    public async Task DecrementTagAffinityAsync(Guid userId, IEnumerable<string> tagNames)
    {
        var key = $"tagaffinity:{userId}";
        foreach (var tag in tagNames)
            await _redis.SortedSetDecrementAsync(key, tag, 1);
    }

    public async Task<Dictionary<string, double>> GetTagAffinityAsync(Guid userId)
    {
        var key = $"tagaffinity:{userId}";
        var entries = await _redis.SortedSetRangeByScoreWithScoresAsync(key, 1);
        return entries.ToDictionary(e => e.Element.ToString(), e => e.Score);
    }
}
