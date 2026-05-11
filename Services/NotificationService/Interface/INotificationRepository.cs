using NotificationService.Entities;

namespace NotificationService.Interface;

public interface INotificationRepository
{
    Task<bool> InsertNotificationAsync(Notification notification);
    Task<List<Notification>> GetNotificationsByDateRangeAsync(string userId, DateTime from, DateTime to);
    Task<bool> UpdateNotificationAsync(Notification notification);
    Task<Notification?> GetNotificationAsync(Guid notificationId, Guid userId);
    Task<int> MarkAllAsReadAsync(Guid userId);
    Task<int> GetUnreadCountAsync(Guid userId);
    Task<bool> DeleteNotificationAsync(Guid notificationId, Guid userId);
}