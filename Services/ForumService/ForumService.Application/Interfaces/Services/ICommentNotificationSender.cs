using ForumService.ForumService.Application.Requests;

namespace ForumService.ForumService.Application.Interfaces.Services;

public interface ICommentNotificationSender
{
    Task SendNotification(CommentNotificationMessage message);
}