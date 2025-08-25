using AutoMapper;
using ForumService.ForumService.Application.Interfaces.Repositories;
using ForumService.ForumService.Domain.Entities;
using ForumService.ForumService.Infrastructure.Data;
using ForumService.ForumService.Infrastructure.Models;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;

namespace ForumService.ForumService.Infrastructure.Repositories;

public class VoteRepository : IVoteRepository
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

    public async Task DeleteThreadVote(ThreadVote vote, Guid forumId)
    {
        ThreadVoteEf threadVoteEf = _mapper.Map<ThreadVoteEf>(vote);
        _context.ThreadVotes.Remove(threadVoteEf);
        await _redis.HashDeleteAsync($"vote:{vote.UserId}:{forumId}", vote.ThreadId.ToString());
    }

    public async Task UpdateThreadVote(ThreadVote vote, Guid forumId)
    {
        ThreadVoteEf threadVoteEf = _mapper.Map<ThreadVoteEf>(vote);
        _context.ThreadVotes.Update(threadVoteEf);
        await _redis.HashSetAsync($"vote:{vote.UserId}:{forumId}", 
            vote.ThreadId.ToString(), vote.DownVote);
    }

    public async Task<ThreadVote?> GetThreadVoteAsync(Guid threadId, Guid userId)
    {
        var threadvote = await _context.ThreadVotes
            .SingleOrDefaultAsync(t => t.ThreadId == threadId && t.UserId == userId);
        
        return _mapper.Map<ThreadVote>(threadvote);
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