using NotificationService.Entities;
using NotificationService.Interface;

namespace NotificationService.Services;

public class NotificationMessageService: INotificationMessageService
{
    private readonly INotificationRepository _repository;

    public NotificationMessageService(INotificationRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<Notification>> GetUserNotification(string userId, DateTime from, DateTime to)
    {
        return await _repository.GetNotificationsByDateRangeAsync(userId, from, to);
    }

    public async Task<bool> InsertNotificationAsync(Notification notification)
    {
        return await _repository.InsertNotificationAsync(notification);
    }

    public async Task<bool> UpdateNotification(Notification notification)
    {
        var isReadNotification = new Notification()
        {
            Id = notification.Id,
            RecipientId = notification.RecipientId,
            CreatedAt = notification.CreatedAt,
            Message = notification.Message,
            IsRead = notification.IsRead,
        };
        bool result = await _repository.UpdateNotificationAsync(isReadNotification);    
        
        return result;
    }

    public async Task<bool> MarkAsRead(Guid notificationId, Guid userId)
    {
        var notification = await _repository.GetNotificationAsync(notificationId, userId);
        if (notification == null || notification.RecipientId != userId)
            return false;

        notification.IsRead = true;
        return await _repository.UpdateNotificationAsync(notification);
    }

    public async Task<int> MarkAllAsRead(Guid userId)
    {
        return await _repository.MarkAllAsReadAsync(userId);
    }

    public async Task<int> GetUnreadCount(Guid userId)
    {
        return await _repository.GetUnreadCountAsync(userId);
    }

    public async Task<bool> DeleteNotification(Guid notificationId, Guid userId)
    {
        return await _repository.DeleteNotificationAsync(notificationId, userId);
    }
}