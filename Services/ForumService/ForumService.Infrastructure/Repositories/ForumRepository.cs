using System.Text.Json;
using AutoMapper;
using ForumService.ForumService.Application.DTOs;
using ForumService.ForumService.Application.Enums;
using ForumService.ForumService.Application.Interfaces.Repositories;
using ForumService.ForumService.Domain.Entities;
using ForumService.ForumService.Infrastructure.Data;
using ForumService.ForumService.Infrastructure.Extensions;
using ForumService.ForumService.Infrastructure.Models;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;

namespace ForumService.ForumService.Infrastructure.Repositories
{
    public class ForumRepository : IForumRepository, IForumQueryRepository
    {
        private static readonly TimeSpan PermissionCacheTtl = TimeSpan.FromMinutes(30);

        private readonly ForumDbContext _context;
        private readonly IDatabase _redis;
        private readonly IMapper _mapper;

        public ForumRepository(ForumDbContext context, IConnectionMultiplexer redis, IMapper mapper)
        {
            _context = context;
            _redis = redis.GetDatabase();
            _mapper = mapper;
        }

        public async Task<Forum?> GetForumByIdAsync(Guid forumId, CancellationToken cancellationToken = default)
        {
            var forum = await _context.Forums
                .AsNoTracking()
                .Where(f => f.Id == forumId)
                .SingleOrDefaultAsync(cancellationToken);
            
            return _mapper.Map<Forum>(forum);
        }

        public async Task<Forum?> GetForumByNameAsync(Guid userId, string name, CancellationToken cancellationToken = default)
        {
            var forum = await _context.Forums
                .AsNoTracking()
                .Where(f => f.Name.ToLower() == name.ToLower() || f.ShortName.ToLower() == name.ToLower())
                .FirstOrDefaultAsync(cancellationToken);
            
            return _mapper.Map<Forum>(forum);
        }

        public async Task<List<Forum>> GetForumsAsync(CancellationToken cancellationToken = default)
        {
            var forums = await _context.Forums.AsNoTracking().ToListAsync(cancellationToken);
            return _mapper.Map<List<Forum>>(forums);
        }

        public async Task<Forum?> InsertForumAsync(Forum forum)
        {
            var existsForum = await _context.Forums.FirstOrDefaultAsync(f => f.Name == forum.Name);
            if (existsForum != null)
                return null;

            ForumEf forumEf = _mapper.Map<ForumEf>(forum);
            await _context.Forums.AddAsync(forumEf);
            return forum;
        }

        public async Task InsertUserToForumAsync(Guid forumId, Guid userId, ForumRole role)
        {
            await _context.ForumUsers.AddAsync(new ForumUserEf
            {
                ForumId = forumId,
                UserId = userId,
                JoinedAt = DateTime.UtcNow,
                Role = role,
                PermissionOverrides = null,
                Active = true
            });
        }

        public async Task<ForumUser?> GetForumUserAsync(Guid forumId, Guid userId, CancellationToken cancellationToken = default)
        {
            var user = await _context.ForumUsers
                .AsNoTracking()
                .Where(f => f.ForumId == forumId && f.UserId == userId)
                .SingleOrDefaultAsync(cancellationToken);

            return user == null ? null : _mapper.Map<ForumUser>(user);
        }

        public async Task UpdateForumUserAsync(ForumUser forumUser)
        {
            var ef = await _context.ForumUsers
                .Where(f => f.ForumId == forumUser.ForumId && f.UserId == forumUser.UserId)
                .SingleOrDefaultAsync();

            if (ef == null) return;

            ef.Role = forumUser.Role;
            ef.PermissionOverrides = forumUser.PermissionOverrides;
            ef.Active = forumUser.Active;
        }

        public async Task RemoveForumUserAsync(Guid forumId, Guid userId)
        {
            var ef = await _context.ForumUsers
                .Where(f => f.ForumId == forumId && f.UserId == userId)
                .SingleOrDefaultAsync();

            if (ef != null)
                _context.ForumUsers.Remove(ef);
        }

        public async Task<List<ForumUser>> GetJoinedForumsByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            var forums = await _context.ForumUsers
                .AsNoTracking()
                .Where(f => f.UserId == userId)
                .Include(f => f.ForumEf)
                .ToListAsync(cancellationToken);

            return _mapper.Map<List<ForumUser>>(forums);
        }

        public async Task<List<ForumUser>> GetForumUsersAsync(Guid forumId, CancellationToken cancellationToken = default)
        {
            var users = await _context.ForumUsers
                .AsNoTracking()
                .Where(f => f.ForumId == forumId)
                .ToListAsync(cancellationToken);

            return _mapper.Map<List<ForumUser>>(users);
        }

        public async Task<List<Guid>> GetJoinedForumIdsAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            return await _context.ForumUsers
                .AsNoTracking()
                .Where(f => f.UserId == userId)
                .Select(f => f.ForumId)
                .ToListAsync(cancellationToken);
        }

        public async Task<Dictionary<Guid, (string Name, string Image)>> GetForumInfoByIdsAsync(IEnumerable<Guid> forumIds, CancellationToken cancellationToken = default)
        {
            return await _context.Forums
                .AsNoTracking()
                .Where(f => forumIds.Contains(f.Id))
                .ToDictionaryAsync(f => f.Id, f => (f.Name, f.ForumImage), cancellationToken);
        }

        // --- Redis permission cache ---

        public async Task SetCachedPermissionsAsync(Guid forumId, Guid userId, int permissions)
        {
            await _redis.StringSetAsync(
                $"perms:{forumId}:{userId}",
                permissions.ToString(),
                PermissionCacheTtl);
        }

        public async ValueTask<int?> GetCachedPermissionsAsync(Guid forumId, Guid userId)
        {
            // Attempt to create the key with zero-allocation intermediate string concatenation
            // Total length: "perms:".Length (6) + 36 (Guid1) + 1 (:) + 36 (Guid2) = 79
            string redisKey = string.Create(79, (forumId, userId), (span, state) =>
            {
                "perms:".AsSpan().CopyTo(span);
                state.forumId.TryFormat(span[6..], out _, "D");
                span[42] = ':';
                state.userId.TryFormat(span[43..], out _, "D");
            });
            
            var value = await _redis.StringGetAsync(redisKey);
            if (value.IsNullOrEmpty) return null;
            
            return (int?)value;
        }

        public async Task InvalidateCachedPermissionsAsync(Guid forumId, Guid userId)
        {
            await _redis.KeyDeleteAsync($"perms:{forumId}:{userId}");
        }

        public async Task<List<ForumTagDto>> GetForumTagCatalogAsync(Guid forumId, CancellationToken cancellationToken = default)
        {
            var rows = await _context.ForumTagCatalog
                .AsNoTracking()
                .Where(t => t.ForumId == forumId)
                .OrderBy(t => t.Name)
                .ToListAsync(cancellationToken);

            return _mapper.Map<List<ForumTagDto>>(rows);
        }

        public async Task<bool> AddForumTagCatalogIfNotExistsAsync(Guid forumId, string name, string color)
        {
            var exists = await _context.ForumTagCatalog
                .AnyAsync(t => t.ForumId == forumId && t.Name.ToLower() == name.ToLower());

            if (exists)
                return false;

            await _context.ForumTagCatalog.AddAsync(new ForumTagCatalogEf
            {
                ForumId = forumId,
                Name = name,
                Color = color,
            });

            return true;
        }

        public async Task<List<Forum>> SearchForumsAsync(string query, CancellationToken cancellationToken = default)
        {
            var lowerQuery = query.ToLower();
            var forums = await _context.Forums
                .AsNoTracking()
                .Where(f => f.Name.ToLower().Contains(lowerQuery) || f.Description.ToLower().Contains(lowerQuery))
                .OrderBy(f => f.Name)
                .Take(20)
                .ToListAsync(cancellationToken);
            return _mapper.Map<List<Forum>>(forums);
        }

        public async ValueTask<List<Forum>> GetRecentVisitForumsAsync(Guid userId,
            CancellationToken cancellationToken = default)
        {
            string redisKey = string.Create(43, userId, (span, state) =>
            {
                state.TryFormat(span, out _, "D");
                span[36] = ':';
                "recent".AsSpan().CopyTo(span[37..]);
            });

            var redisValues = await _redis.SortedSetRangeByRankAsync(
                redisKey,
                start: 0,
                stop: -1,
                order: Order.Descending);

            return redisValues
                .Select(v =>
                {
                    try
                    {
                        var json = (string?)v;
                        return json != null ? JsonSerializer.Deserialize<ForumEf>(json) : null;
                    }
                    catch { return null; }
                })
                .Where(f => f != null)
                .Select(f => _mapper.Map<Forum>(f!))
                .ToList();
        }

        public async Task WriteVisitAsync(Guid userId, Guid forumId, CancellationToken cancellationToken = default)
        {
            var forum = await _context.Forums
                .AsNoTracking()
                .Where(f => f.Id == forumId)
                .FirstOrDefaultAsync(cancellationToken);

            if (forum != null)
                await _redis.WriteVisitForum(userId, forum);
        }
    }
}
