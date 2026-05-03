using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.SignalR;
using NotificationService.DTO;
using NotificationService.Entities;
using NotificationService.Interface;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace NotificationService.Messaging;

public class RabbitMqConsumer: BackgroundService
{
    private readonly IHubContext<NotificationHub> _hubContext;
    private readonly ILogger<RabbitMqConsumer> _logger;
    private readonly IConfiguration _config;
    private readonly INotificationMessageService _notificationMessageService;
    private IConnection? _connection;
    private IChannel? _channel;

    public RabbitMqConsumer(IHubContext<NotificationHub> hubContext, ILogger<RabbitMqConsumer> logger
        , IConfiguration config,  INotificationMessageService notificationMessageService)
    {
        _hubContext = hubContext;
        _logger = logger;
        _config = config;
        _notificationMessageService = notificationMessageService;
    }

    public override async Task StartAsync(CancellationToken cancellationToken)
    {
        var hostName = _config["RabbitMQ:HostName"] ?? "localhost";
        var port = _config.GetValue("RabbitMQ:Port", 5672);
        var factory = new ConnectionFactory
        {
            HostName = hostName,
            Port = port,
            UserName = _config["RabbitMQ:UserName"] ?? "guest",
            Password = _config["RabbitMQ:Password"] ?? "guest",
            VirtualHost = _config["RabbitMQ:VirtualHost"] ?? "/"
        };
        if (_config.GetValue("RabbitMQ:UseTls", false))
        {
            factory.Ssl = new SslOption { Enabled = true, ServerName = hostName };
        }
        _connection = await factory.CreateConnectionAsync();
        _channel = await _connection.CreateChannelAsync();  
        
        await _channel.QueueDeclareAsync(
            queue:"notification", 
            durable: true, 
            exclusive: false, 
            autoDelete: false, 
            arguments: null, 
            passive: false, 
            cancellationToken: cancellationToken
        );
        
        await base.StartAsync(cancellationToken);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var consumer = new AsyncEventingBasicConsumer(_channel);

        consumer.ReceivedAsync += async (model, ea) =>
        {
            try
            {
                var body = ea.Body.ToArray();
                var messageJson = Encoding.UTF8.GetString(body);
                var notification = JsonSerializer.Deserialize<NotificationMessageDto>(messageJson);
                if (notification != null &&  !string.IsNullOrEmpty(notification.Message))
                {
                    await _hubContext.Clients.User(notification.UserId)
                        .SendAsync("ReceiveNotification", notification.Message);

                    var newNotification = new Notification()
                    {
                        Id = Guid.NewGuid(),
                        Message = notification.Message,
                        RecipientId = Guid.Parse(notification.UserId),
                        CreatedAt = DateTime.UtcNow,
                        IsRead = false
                    };
                    await _notificationMessageService.InsertNotificationAsync(newNotification);
                    _logger.LogInformation("Notification {NotificationId} has been received", newNotification.Id);
                    await _channel.BasicAckAsync(ea.DeliveryTag, false);
                }
                else
                {
                    _logger.LogWarning("Cannot deserialize notification message");
                    await _channel.BasicNackAsync(ea.DeliveryTag, false, false);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing notification message");
                await _channel.BasicNackAsync(ea.DeliveryTag, false, false);
            }
        };

        await _channel.BasicConsumeAsync(
            queue: "notification",
            autoAck: false,
            consumer: consumer,
            cancellationToken: stoppingToken
        );
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("RabbitMQ consumer stopping");
        try
        {
            if (_channel != null)
                await _channel.DisposeAsync();
            if (_connection != null)
                await _connection.DisposeAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during RabbitMQ consumer stopping");
        }
        await base.StopAsync(cancellationToken);
    }
}
