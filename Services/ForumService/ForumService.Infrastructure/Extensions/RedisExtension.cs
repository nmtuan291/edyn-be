using System.Text.Json;
using ForumService.ForumService.Infrastructure.Models;
using StackExchange.Redis;

namespace ForumService.ForumService.Infrastructure.Extensions;

public static class RedisExtension
{
    public static async Task WriteVisitForum(this IDatabase redis, Guid userId, ForumEf forum) 
    {
        try
        {
            string key = $"{userId}:recent";
            string recentForum = JsonSerializer.Serialize(forum);
            double score = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

            await redis.SortedSetAddAsync(key, recentForum, score);
                
            // Cap the list to the top 10 most recent forums
            await redis.SortedSetRemoveRangeByRankAsync(key, 0, -11);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Redis caching failed: {ex.Message}");
        }
    }
}