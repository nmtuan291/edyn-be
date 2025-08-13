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
            RecipentId = notification.RecipentId,
            CreatedAt = notification.CreatedAt,
            Message = notification.Message,
            IsRead = notification.IsRead,
        };
        bool result = await _repository.UpdateNotificationAsync(isReadNotification);    
        
        return result;
    }
}