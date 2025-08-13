using System.Text;
using System.Text.Json;
using ChatService.ChatService.Application.Interfaces.Messaging;
using ChatService.ChatService.Domain.Entities;
using Microsoft.AspNetCore.SignalR;
using ChatService.ChatService.API.Hubs;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace ChatService.ChatService.Infrastructure.Messaging;

public class ChatConsumer: IAsyncDisposable, IChatConsumer
{
    private readonly RabbitMqConnectionFactory _factory;
    private readonly SemaphoreSlim _semaphore = new (1, 1);
    private Dictionary<string, IChannel>  _channels = new();
    private readonly IHubContext<ChatHub> _chatHubContext;
    private readonly string _exchangeName = "chat.direct";

    public ChatConsumer(RabbitMqConnectionFactory factory, IHubContext<ChatHub> chatHubContext)
    {
        _factory = factory;
        _chatHubContext = chatHubContext;
    }

    public async Task StartListeningAsync(string userId)
    {
        await _semaphore.WaitAsync();
        string queueName = $"chat.{userId}";
        if (_channels.ContainsKey(userId))
            return;

        try
        {
            var channel = await _factory.CreateChannelAsync();
            _channels[userId] = channel;

            await channel.ExchangeDeclareAsync(
                exchange:  _exchangeName,
                type: "direct",
                durable: true,
                autoDelete: false
            );

            await channel.QueueDeclareAsync(
                queue: queueName,
                durable: true,
                autoDelete: false,
                exclusive: false
            );

            await channel.QueueBindAsync(
                queue: queueName,
                exchange: _exchangeName,
                routingKey: userId
            );

            var consumer = new AsyncEventingBasicConsumer(channel);
            consumer.ReceivedAsync += async (model, ea) =>
            {
                try
                {
                    var body = ea.Body.ToArray();
                    var bodyString = Encoding.UTF8.GetString(body);
                    var bodyJson = JsonSerializer.Deserialize<Message>(bodyString);
                    await _chatHubContext.Clients.User(userId).SendAsync("ReceiveMessage", bodyJson);
                    await channel.BasicAckAsync(deliveryTag: ea.DeliveryTag, multiple: false);
                }
                catch (Exception ex)
                {
                    await channel.BasicNackAsync(deliveryTag: ea.DeliveryTag, multiple: false, requeue: true);
                }
        
            };

            await channel.BasicConsumeAsync(
                queue: queueName,
                autoAck: false,
                consumer: consumer
            );
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public async Task StopListeningAsync(string userId)
    {
        await  _semaphore.WaitAsync();
        try
        {
            if (!_channels.ContainsKey(userId))
                return;

            var channel = _channels[userId];
            await channel.DisposeAsync();
            _channels.Remove(userId);
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public async ValueTask DisposeAsync()
    {
        await _semaphore.WaitAsync();
        try
        {
            foreach (var channel in _channels.Values)
            {
                await channel.DisposeAsync();
            }
            _channels.Clear();
        }
        finally
        {
            _semaphore.Release();
            _semaphore.Dispose();
        }
    }
}