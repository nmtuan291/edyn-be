using NotificationService.Entities;

namespace NotificationService.Interface;

public interface INotificationMessageService
{
    Task<List<Notification>> GetUserNotification(string userId, DateTime from, DateTime to);
    Task<bool> InsertNotificationAsync(Notification notification);
    Task<bool> UpdateNotification(Notification notification);
    Task<bool> MarkAsRead(Guid notificationId, Guid userId);
    Task<int> MarkAllAsRead(Guid userId);
    Task<int> GetUnreadCount(Guid userId);
    Task<bool> DeleteNotification(Guid notificationId, Guid userId);
}