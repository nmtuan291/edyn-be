using System.Text;
using Microsoft.AspNetCore.SignalR;
using Newtonsoft.Json;
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
    private IConnection _connection;
    private IChannel _channel;

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
        var factory = new ConnectionFactory() { HostName = _config["RabbitMQ:HostName"] };
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
                var notification = JsonConvert.DeserializeObject<NotificationMessageDto>(messageJson);
                if (notification != null &&  !string.IsNullOrEmpty(notification.Message))
                {
                    await _hubContext.Clients.User(notification.UserId)
                        .SendAsync("ReceiveNotification", notification.Message);

                    var newNotification = new Notification()
                    {
                        Id = Guid.NewGuid(),
                        Message = notification.Message,
                        RecipentId = Guid.Parse(notification.UserId),
                        CreatedAt = DateTime.Now,
                        IsRead = false
                    };
                    await _notificationMessageService .InsertNotificationAsync(newNotification);
                    Console.WriteLine($"{newNotification.Id} has been received");
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
                _logger.LogError(ex.Message);
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
            await _channel.DisposeAsync();
            await _connection.DisposeAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during RabbitMQ consumer stopping");
        }
        await base.StopAsync(cancellationToken);
    }
}