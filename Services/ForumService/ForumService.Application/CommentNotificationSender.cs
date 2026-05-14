using ForumService.ForumService.Application.DTOs;
using ForumService.ForumService.Application.Interfaces.Services;
using ForumService.ForumService.Application.Requests;
using ForumService.ForumService.Infrastructure.Messaging;

namespace ForumService.ForumService.Application;

public class CommentNotificationSender: ICommentNotificationSender
{
    private readonly ILogger<CommentNotificationSender> _logger;
    private readonly ILogger<RabbitMqProducer> _producerLogger;
    private readonly IConfiguration _configuration;
    
    public CommentNotificationSender(ILogger<CommentNotificationSender> logger, ILogger<RabbitMqProducer> producerLogger, IConfiguration configuration)
    {
        _logger = logger;
        _producerLogger = producerLogger;
        _configuration = configuration;
    }

    public async Task SendNotification(CommentNotificationMessage message)
    {
        string connectionString = _configuration["RabbitMQ:ConnectionString"] ?? _configuration["RabbitMQ:HostName"] ?? "localhost";
        await using var producer = await RabbitMqProducer.CreateAsync(connectionString, _producerLogger);
        var payload = new NotificationMessageDto()
        {
            UserId = message.UserId,
            Message = $"Người dùng {message.UserName} đã trả lời bình luận của bạn",
            CreatedOn = DateTime.UtcNow,
        };
        await producer.SendAsync("notification", payload);
    }
}