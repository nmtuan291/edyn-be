using System.Text.Json;
using NotificationService.Entities;
using NotificationService.Interface;
using StackExchange.Redis;

namespace NotificationService.Repositories;

public class NotificationRepository: INotificationRepository
{
    private readonly IDatabase _redis;

    public NotificationRepository(IConnectionMultiplexer redis)
    {
        _redis = redis.GetDatabase();
    }

    public async Task<bool> InsertNotificationAsync(Notification notification)
    {
        var json = JsonSerializer.Serialize(notification);
        bool success = await _redis.StringSetAsync($"notification:{notification.RecipentId}:{notification.Id}", json);
        if (success)
        {
            double timestamp = notification.CreatedAt.Ticks;
            await _redis.SortedSetAddAsync($"user:{notification.RecipentId}:notifications_by_time",
                notification.Id.ToString(), timestamp);
            return true;
        }
        
        return success;
    }

    public async Task<List<Notification>> GetNotificationsByDateRangeAsync(string userId, DateTime from, DateTime to)
    {
        Console.WriteLine($"GetNotificationsByDateRangeAsync -------------------------------------------");
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
            .Select(n => JsonSerializer.Deserialize<Notification>(n))
            .ToList();
    }

    public async Task<bool> UpdateNotificationAsync(Notification notification)
    {
        var json = JsonSerializer.Serialize(notification);
        bool success = await _redis.StringSetAsync($"notification:{notification.RecipentId}:{notification.Id}", json);
        return success;
    }
}