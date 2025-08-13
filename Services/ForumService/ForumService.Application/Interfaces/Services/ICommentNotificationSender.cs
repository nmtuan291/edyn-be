namespace ForumService.ForumService.Application.Interfaces.Services;

public interface ICommentNotificationSender
{
    Task SendNotification(string userId, string userName, string content, string  threadId);
}