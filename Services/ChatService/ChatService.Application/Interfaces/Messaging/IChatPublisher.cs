namespace ChatService.ChatService.Application.Interfaces.Messaging;

public interface IChatPublisher
{
    Task SendMessageAsync(object message, string fromUserId, string toUserId);
}