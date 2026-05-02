using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using ChatService.ChatService.Application.Interfaces.Messaging;

namespace ChatService.ChatService.API.Hubs;

[Authorize]
public class ChatHub : Hub
{
    private readonly IChatConsumer _chatConsumer;
    private readonly ILogger<ChatHub> _logger;

    public ChatHub(IChatConsumer chatConsumer, ILogger<ChatHub> logger)
    {
        _chatConsumer = chatConsumer;
        _logger = logger;
    }

    public override async Task OnConnectedAsync()
    {
        var userId = Context.UserIdentifier;
        if (!string.IsNullOrEmpty(userId))
        {
            try
            {
                await _chatConsumer.StartListeningAsync(userId);
                _logger.LogInformation("User {UserId} connected to chat hub and started listening for messages", userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to start listening for user {UserId}", userId);
            }
        }
        else
        {
            _logger.LogWarning("User connected to chat hub but UserIdentifier is null");
        }
        
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = Context.UserIdentifier;
        if (!string.IsNullOrEmpty(userId))
        {
            try
            {
                await _chatConsumer.StopListeningAsync(userId);
                _logger.LogInformation("User {UserId} disconnected from chat hub and stopped listening for messages", userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to stop listening for user {UserId}", userId);
            }
        }
        
        await base.OnDisconnectedAsync(exception);
    }
    
} 