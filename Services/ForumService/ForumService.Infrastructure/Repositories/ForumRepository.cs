using ForumService.ForumService.Application.Interfaces.Repositories;
using ForumService.ForumService.Domain.Entities;
using ForumService.ForumService.Infrastructure.Data;
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

        public ForumRepository(ForumDbContext context, IConnectionMultiplexer redis)
        {
            _context = context;
            _redis = redis.GetDatabase();
        }

        public async Task<Forum?> GetForumByIdAsync(Guid forumId)
        {
            return await _context.Forums
                .Where(f => f.Id == forumId)
                .SingleOrDefaultAsync();
        }

        public async Task<Forum?> GetForumByNameAsync(string name)
        {
            return await _context.Forums
                .Where(f => f.Name == name) 
                .SingleOrDefaultAsync();
        }
        
        public async Task<IEnumerable<Forum>> GetForumsAsync()
        {
            return await _context.Forums.ToListAsync();
        }
        
        public async Task<Forum?> InsertForumAsync(Forum forum)
        {
            var existsForum =  await _context.Forums.FirstOrDefaultAsync(f => f.Name == forum.Name);
            if (existsForum != null)
                return null;
            
            await _context.Forums.AddAsync(forum);
            return forum;
        }

        public async Task InsertUserToForumAsync(Guid forumId, Guid userId, string permissions, bool isModerator)
        {
            await _context.ForumUsers.AddAsync(new ForumUser
            {
                ForumId = forumId, 
                UserId = userId,
                JoinedAt = DateTime.UtcNow,
                Moderator = isModerator,
                Active = true
            });
            Console.WriteLine($"Created----------------------------{forumId}:{userId}");
            await _redis.StringSetAsync($"{forumId}:{userId}", permissions);
        }

        public async Task<string?> GetUserPermissionAsync(Guid forumId, Guid userId)
        {
            Console.WriteLine($"----------------------------{forumId}:{userId}");
            return await _redis.StringGetAsync($"{forumId}:{userId}");
        }
    }
}
