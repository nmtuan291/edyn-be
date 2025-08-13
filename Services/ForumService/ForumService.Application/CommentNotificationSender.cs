using ForumService.ForumService.Application.DTOs;
using ForumService.ForumService.Application.Interfaces.Services;
using ForumService.ForumService.Infrastructure.Messaging;
using Microsoft.VisualStudio.TestPlatform.CommunicationUtilities;

namespace ForumService.ForumService.Application;

public class CommentNotificationSender: ICommentNotificationSender
{
    public readonly ILogger<CommentNotificationSender> _Logger;
    public readonly ILogger<RabbitMqProducer> _producerLogger;
    public readonly IConfiguration _configuration;
    
    public CommentNotificationSender(ILogger<CommentNotificationSender> logger, ILogger<RabbitMqProducer> producerLogger, IConfiguration configuration)
    {
        _Logger = logger;
        _producerLogger = producerLogger;
        _configuration = configuration;
    }

    public async Task SendNotification(string userId, string userName, string content, string threadId)
    {
        string hostName = _configuration["RabbitMQ:HostName"] ?? "localhost";
        var producer = await RabbitMqProducer.CreateAsync(hostName,  _producerLogger);
        var message = new NotificationMessageDto()
        {
            UserId = userId,
            Message = $"Người dùng {userName} đã trả lời bình luận của bạn",
            CreatedOn = DateTime.UtcNow,
        };
        await producer.SendAsync("notification", message);
    }
}