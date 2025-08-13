using NotificationService.Entities;

namespace NotificationService.Interface;

public interface INotificationMessageService
{
    Task<List<Notification>> GetUserNotification(string userId, DateTime from, DateTime to);
    Task<bool> InsertNotificationAsync(Notification notification);
    Task<bool> UpdateNotification(Notification notification);
}