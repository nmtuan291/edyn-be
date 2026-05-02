namespace ForumService.ForumService.Application.Requests;

public record CommentNotificationMessage(
    string UserId,
    string UserName,
    string Content,
    string ThreadId);
