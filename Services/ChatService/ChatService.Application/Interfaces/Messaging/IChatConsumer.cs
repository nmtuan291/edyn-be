namespace ChatService.ChatService.Application.Interfaces.Messaging;

public interface IChatConsumer
{
    Task StartListeningAsync(string userId);
    Task StopListeningAsync(string userId);
}