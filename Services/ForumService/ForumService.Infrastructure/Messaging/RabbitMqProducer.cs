using System.Text;
using ForumService.ForumService.Application.DTOs;
using Microsoft.EntityFrameworkCore.Metadata;
using Newtonsoft.Json;
using RabbitMQ.Client;

namespace ForumService.ForumService.Infrastructure.Messaging;

public class RabbitMqProducer: IAsyncDisposable
{
    private readonly IConnection _connection;
    private readonly IChannel _channel;
    private readonly ILogger<RabbitMqProducer> _logger;

    public RabbitMqProducer(IConnection connection, IChannel channel,  ILogger<RabbitMqProducer> logger)
    {
        _connection = connection;
        _channel = channel;
        _logger = logger;
    }

    public static async Task<RabbitMqProducer> CreateAsync(string hostName, ILogger<RabbitMqProducer> logger)
    {
        var factory = new ConnectionFactory() { HostName = hostName };
        var connection = await factory.CreateConnectionAsync();
        var channel = await connection.CreateChannelAsync();
        return new RabbitMqProducer(connection, channel, logger);
    }

    public async Task SendAsync(string queue, NotificationMessageDto message)
    {
        try
        {
            await _channel.QueueDeclareAsync(queue, true, false, false, null);
            var bodyJson = JsonConvert.SerializeObject(message);
            var body = Encoding.UTF8.GetBytes(bodyJson);
            await _channel.BasicPublishAsync(
                "",
                queue,
                body.AsMemory(),
                CancellationToken.None
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending message");
        }
            
    }

    public async ValueTask DisposeAsync()
    {
        try
        {
            await _channel.DisposeAsync();
            await _connection.DisposeAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error closing connection");
        }
    }
}
