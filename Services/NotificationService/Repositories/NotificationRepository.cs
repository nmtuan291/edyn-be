using System.Text.Json;
using NotificationService.Entities;
using NotificationService.Interface;
using StackExchange.Redis;

namespace NotificationService.Repositories;

public class NotificationRepository: INotificationRepository
{
    private readonly IDatabase _redis;
    private readonly ILogger<NotificationRepository> _logger;

    public NotificationRepository(IConnectionMultiplexer redis, ILogger<NotificationRepository> logger)
    {
        _redis = redis.GetDatabase();
        _logger = logger;
    }

    public async Task<bool> InsertNotificationAsync(Notification notification)
    {
        var json = JsonSerializer.Serialize(notification);
        bool success = await _redis.StringSetAsync($"notification:{notification.RecipientId}:{notification.Id}", json);
        if (success)
        {
            double timestamp = notification.CreatedAt.Ticks;
            await _redis.SortedSetAddAsync($"user:{notification.RecipientId}:notifications_by_time",
                notification.Id.ToString(), timestamp);
            return true;
        }
        
        return success;
    }

    public async Task<List<Notification>> GetNotificationsByDateRangeAsync(string userId, DateTime from, DateTime to)
    {
        _logger.LogDebug("GetNotificationsByDateRangeAsync called");
        var notificationsTimestamp = await _redis.SortedSetRangeByScoreAsync(
            $"user:{userId}:notifications_by_time", 
            from.Ticks, 
            to.Ticks
        );
        
        if (!notificationsTimestamp.Any())
            return new List<Notification>();
        
        var keys = notificationsTimestamp.Select(id => (RedisKey) $"notification:{userId}:{id}").ToArray();
        var notifications = await _redis.StringGetAsync(keys);

        return notifications
            .Where(n => n.HasValue && !string.IsNullOrEmpty(n))
            .Select(n => JsonSerializer.Deserialize<Notification>(n!))
            .Where(n => n != null)
            .ToList()!;
    }

    public async Task<bool> UpdateNotificationAsync(Notification notification)
    {
        var json = JsonSerializer.Serialize(notification);
        bool success = await _redis.StringSetAsync($"notification:{notification.RecipientId}:{notification.Id}", json);
        return success;
    }

    public async Task<Notification?> GetNotificationAsync(Guid notificationId, Guid userId)
    {
        var json = await _redis.StringGetAsync($"notification:{userId}:{notificationId}");
        if (!json.HasValue || string.IsNullOrEmpty(json))
            return null;
        return JsonSerializer.Deserialize<Notification>(json!);
    }
}