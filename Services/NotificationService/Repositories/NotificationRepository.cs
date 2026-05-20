using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using NotificationService.Data;
using NotificationService.Entities;
using NotificationService.Interface;
using StackExchange.Redis;

namespace NotificationService.Repositories;

public class NotificationRepository : INotificationRepository
{
    private readonly NotificationDbContext _db;
    private readonly IDatabase _redis;
    private readonly ILogger<NotificationRepository> _logger;

    public NotificationRepository(
        NotificationDbContext db, 
        IConnectionMultiplexer redis, 
        ILogger<NotificationRepository> logger)
    {
        _db = db;
        _redis = redis.GetDatabase();
        _logger = logger;
    }

    public async Task<bool> InsertNotificationAsync(Notification notification)
    {
        _logger.LogInformation("Inserting notification {NotificationId} for user {UserId} into PostgreSQL", notification.Id, notification.RecipientId);
        
        await _db.Notifications.AddAsync(notification);
        var result = await _db.SaveChangesAsync() > 0;

        if (result)
        {
            // Cache the individual notification
            var json = JsonSerializer.Serialize(notification);
            await _redis.StringSetAsync($"notification:{notification.RecipientId}:{notification.Id}", json);

            // Add to sorted set to preserve timeline
            await _redis.SortedSetAddAsync($"user:{notification.RecipientId}:notifications_by_time", notification.Id.ToString(), notification.CreatedAt.Ticks);

            // Proactively increment cached unread count in Redis if it exists
            var unreadKey = $"user:{notification.RecipientId}:unread_count";
            if (await _redis.KeyExistsAsync(unreadKey))
            {
                await _redis.StringIncrementAsync(unreadKey);
                _logger.LogDebug("Incremented unread count in Redis cache for user {UserId}", notification.RecipientId);
            }
        }

        return result;
    }

    public async Task<List<Notification>> GetNotificationsByDateRangeAsync(string userId, DateTime from, DateTime to)
    {
        if (!Guid.TryParse(userId, out var parsedUserId))
            return new List<Notification>();

        // Try to read from Redis cache
        var notificationIds = await _redis.SortedSetRangeByScoreAsync($"user:{userId}:notifications_by_time", from.Ticks, to.Ticks);
        if (notificationIds.Any())
        {
            var keys = notificationIds.Select(id => (RedisKey)$"notification:{userId}:{id}").ToArray();
            var redisValues = await _redis.StringGetAsync(keys);
            
            var cachedNotifications = redisValues
                .Where(n => n.HasValue && !string.IsNullOrEmpty(n))
                .Select(n => JsonSerializer.Deserialize<Notification>(n!))
                .Where(n => n != null)
                .Select(n => n!)
                .OrderByDescending(n => n.CreatedAt)
                .ToList();
                
            if (cachedNotifications.Any())
            {
                _logger.LogDebug("Returned notifications from Redis cache for user {UserId}", userId);
                return cachedNotifications;
            }
        }

        // Cache miss or empty: fallback to PostgreSQL
        _logger.LogInformation("Cache miss. Fetching notifications for user {UserId} between {From} and {To} from PostgreSQL", userId, from, to);
        var dbNotifications = await _db.Notifications
            .Where(n => n.RecipientId == parsedUserId && n.CreatedAt >= from && n.CreatedAt <= to)
            .OrderByDescending(n => n.CreatedAt)
            .ToListAsync();
            
        // Populate Redis cache for these fetched notifications
        foreach (var n in dbNotifications)
        {
            var json = JsonSerializer.Serialize(n);
            await _redis.StringSetAsync($"notification:{userId}:{n.Id}", json);
            await _redis.SortedSetAddAsync($"user:{userId}:notifications_by_time", n.Id.ToString(), n.CreatedAt.Ticks);
        }
        
        return dbNotifications;
    }

    public async Task<bool> UpdateNotificationAsync(Notification notification)
    {
        _logger.LogInformation("Updating notification {NotificationId} for user {UserId} in PostgreSQL", notification.Id, notification.RecipientId);
        
        _db.Notifications.Update(notification);
        var result = await _db.SaveChangesAsync() > 0;

        if (result)
        {
            // Update individual notification cache
            var cacheKey = $"notification:{notification.RecipientId}:{notification.Id}";
            if (await _redis.KeyExistsAsync(cacheKey))
            {
                var json = JsonSerializer.Serialize(notification);
                await _redis.StringSetAsync(cacheKey, json);
            }

            // Invalidate cached unread count
            var unreadKey = $"user:{notification.RecipientId}:unread_count";
            await _redis.KeyDeleteAsync(unreadKey);
        }

        return result;
    }

    public async Task<Notification?> GetNotificationAsync(Guid notificationId, Guid userId)
    {
        var cacheKey = $"notification:{userId}:{notificationId}";
        var cached = await _redis.StringGetAsync(cacheKey);
        if (cached.HasValue && !string.IsNullOrEmpty(cached))
        {
            return JsonSerializer.Deserialize<Notification>(cached!);
        }

        _logger.LogDebug("Cache miss for specific notification. Fetching {NotificationId} for user {UserId} from PostgreSQL", notificationId, userId);
        return await _db.Notifications
            .FirstOrDefaultAsync(n => n.Id == notificationId && n.RecipientId == userId);
    }

    public async Task<int> MarkAllAsReadAsync(Guid userId)
    {
        _logger.LogInformation("Marking all notifications as read for user {UserId} in PostgreSQL", userId);
        
        int count = await _db.Notifications
            .Where(n => n.RecipientId == userId && !n.IsRead)
            .ExecuteUpdateAsync(s => s.SetProperty(n => n.IsRead, true));

        // Invalidate sorted set so next read hits DB and repopulates cache with accurate "read" statuses
        await _redis.KeyDeleteAsync($"user:{userId}:notifications_by_time");

        // Explicitly set unread count in Redis to 0
        var cacheKey = $"user:{userId}:unread_count";
        await _redis.StringSetAsync(cacheKey, 0);

        return count;
    }

    public async Task<int> GetUnreadCountAsync(Guid userId)
    {
        var cacheKey = $"user:{userId}:unread_count";
        
        // Try to read from Redis cache
        var cachedCount = await _redis.StringGetAsync(cacheKey);
        if (cachedCount.HasValue)
        {
            if (int.TryParse(cachedCount, out var count))
            {
                return count;
            }
        }

        // Cache miss: read from PostgreSQL
        int dbCount = await _db.Notifications
            .CountAsync(n => n.RecipientId == userId && !n.IsRead);

        // Populate Redis cache
        await _redis.StringSetAsync(cacheKey, dbCount);
        
        return dbCount;
    }

    public async Task<bool> DeleteNotificationAsync(Guid notificationId, Guid userId)
    {
        _logger.LogInformation("Deleting notification {NotificationId} for user {UserId} from PostgreSQL", notificationId, userId);
        
        var notification = await GetNotificationAsync(notificationId, userId);
        if (notification == null)
            return false;

        _db.Notifications.Remove(notification);
        var result = await _db.SaveChangesAsync() > 0;

        if (result)
        {
            // Clean up all related cache keys
            await _redis.KeyDeleteAsync($"user:{userId}:unread_count");
            await _redis.KeyDeleteAsync($"notification:{userId}:{notificationId}");
            await _redis.SortedSetRemoveAsync($"user:{userId}:notifications_by_time", notificationId.ToString());
        }

        return result;
    }
}