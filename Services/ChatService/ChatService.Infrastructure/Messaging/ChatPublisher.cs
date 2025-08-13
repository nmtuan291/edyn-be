using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using ChatService.ChatService.Application.Interfaces.Messaging;
using RabbitMQ.Client;

namespace ChatService.ChatService.Infrastructure.Messaging;

public class ChatPublisher: IChatPublisher
{
    private readonly RabbitMqConnectionFactory _connectionFactory;
    
    public ChatPublisher(RabbitMqConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task SendMessageAsync(object message, string fromUserId, string toUserId)
    {
        using var channel = await _connectionFactory.CreateChannelAsync();
        string exchangeName = "chat.direct";

        await channel.ExchangeDeclareAsync(
            exchange: exchangeName,
            type: "direct",
            durable: true,
            autoDelete: false
        );
        
        var json = JsonSerializer.Serialize(message);
        var bodyArray = Encoding.UTF8.GetBytes(json);

        var properties = new BasicProperties()
        {
            Persistent = true,
            Timestamp = new AmqpTimestamp(DateTimeOffset.UtcNow.ToUnixTimeSeconds()),
        };

        await channel.BasicPublishAsync(
            exchange: exchangeName,
            routingKey: toUserId,
            basicProperties: properties,
            body: bodyArray.AsMemory(),
            mandatory: true
        );
    }
}