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
            // Start listening for RabbitMQ messages when user connects to SignalR
            await _chatConsumer.StartListeningAsync(userId);
            _logger.LogInformation($"User {userId} connected to chat hub and started listening for messages");
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
            // Stop listening for RabbitMQ messages when user disconnects from SignalR
            await _chatConsumer.StopListeningAsync(userId);
            _logger.LogInformation($"User {userId} disconnected from chat hub and stopped listening for messages");
        }
        
        await base.OnDisconnectedAsync(exception);
    }
    
} 