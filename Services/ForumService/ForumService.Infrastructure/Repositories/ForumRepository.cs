using AutoMapper;
using ForumService.ForumService.Application.Interfaces.Repositories;
using ForumService.ForumService.Domain.Entities;
using ForumService.ForumService.Infrastructure.Data;
using ForumService.ForumService.Infrastructure.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Microsoft.Extensions.Caching.Distributed;
using StackExchange.Redis;

namespace ForumService.ForumService.Infrastructure.Repositories
{
    public class ForumRepository: IForumRepository
    {
        private readonly ForumDbContext _context;
        private readonly IDatabase _redis;
        private readonly IMapper _mapper;

        public ForumRepository(ForumDbContext context, IConnectionMultiplexer redis, IMapper mapper)
        {
            _context = context;
            _redis = redis.GetDatabase();
            _mapper = mapper;
        }

        public async Task<Forum?> GetForumByIdAsync(Guid forumId)
        {
            var forum = await _context.Forums
                .Where(f => f.Id == forumId)
                .SingleOrDefaultAsync();
            
            return _mapper.Map<Forum>(forum);
        }

        public async Task<Forum?> GetForumByNameAsync(string name)
        {
            var forum = await _context.Forums
                .Where(f => f.Name == name) 
                .SingleOrDefaultAsync();
            
            return _mapper.Map<Forum>(forum);
        }
        
        public async Task<List<Forum>> GetForumsAsync()
        {
            var forums = await _context.Forums.ToListAsync();
            return _mapper.Map<List<Forum>>(forums);
        }
        
        public async Task<Forum?> InsertForumAsync(Forum forum)
        {
            var existsForum =  await _context.Forums.FirstOrDefaultAsync(f => f.Name == forum.Name);
            if (existsForum != null)
                return null;
            
            ForumEf forumEf = _mapper.Map<ForumEf>(forum);
            await _context.Forums.AddAsync(forumEf);
            return forum;
        }

        public async Task InsertUserToForumAsync(Guid forumId, Guid userId, string permissions, bool isModerator)
        {
            await _context.ForumUsers.AddAsync(new ForumUserEf
            {
                ForumId = forumId, 
                UserId = userId,
                JoinedAt = DateTime.UtcNow,
                Moderator = isModerator,
                Active = true
            });
            await _redis.StringSetAsync($"{forumId}:{userId}", permissions);
        }

        public async Task<string?> GetUserPermissionAsync(Guid forumId, Guid userId)
        {
            return await _redis.StringGetAsync($"{forumId}:{userId}");
        }

        public async Task<List<ForumUser>> GetJoinedForumsByUserIdAsync(Guid userId)
        {
            var forums = await _context.ForumUsers
                .Where(f => f.UserId == userId)
                .Include(f => f.ForumEf)
                .ToListAsync();
            
            return _mapper.Map<List<ForumUser>>(forums);
        }
    }
}
