using System.Text;
using System.Text.Json;
using RabbitMQ.Client;

namespace ForumService.ForumService.Infrastructure.Messaging;

public class TelemetryPublisherService : BackgroundService
{
    private readonly BoundedChannelBuffer<TelemetryLog> _buffer;
    private readonly RabbitMqConnectionFactory _connectionFactory;

    public TelemetryPublisherService(BoundedChannelBuffer<TelemetryLog> buffer, RabbitMqConnectionFactory connectionFactory)
    {
        _buffer = buffer;
        _connectionFactory = connectionFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var options = new CreateChannelOptions(
            publisherConfirmationsEnabled: false,
            publisherConfirmationTrackingEnabled: false,
            outstandingPublisherConfirmationsRateLimiter: null);
            
        await using var channel = await _connectionFactory.CreateChannel(options, stoppingToken);
        
        await channel.ExchangeDeclareAsync(
            exchange: "analytics.interactions", 
            "direct",
            durable: true,
            cancellationToken: stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            if (await _buffer.Reader.WaitToReadAsync(stoppingToken))
            {
                while (_buffer.Reader.TryRead(out var telemetryLog))
                {
                    string json = JsonSerializer.Serialize(telemetryLog);
                    byte[] bodyBytes = Encoding.UTF8.GetBytes(json);
                    
                    await channel.BasicPublishAsync(
                        exchange: "analytics.interactions",
                        routingKey:"analytics.interactions",
                        body: bodyBytes,
                        cancellationToken: stoppingToken);
                }
            }
        }
    }
}