using RabbitMQ.Client;

namespace ForumService.ForumService.Infrastructure.Messaging;

public class RabbitMqConnectionFactory(IConnectionFactory connectionFactory) : IAsyncDisposable
{
    private IConnection? _connection;
    private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);
    private bool _disposed;
    
    protected async Task CreateConnection(CancellationToken cancellationToken)
    {
        if (_connection != null)
            return;
        
        await _semaphore.WaitAsync(cancellationToken);
        try
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(RabbitMqConnectionFactory));

            if (_connection == null)
            {
                await _semaphore.WaitAsync(cancellationToken);
                _connection = await connectionFactory.CreateConnectionAsync(cancellationToken);
            }
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public async Task<IChannel> CreateChannel(CreateChannelOptions options, CancellationToken cancellationToken)
    {
        if (_connection == null)
            await CreateConnection(cancellationToken);

        return await _connection.CreateChannelAsync(options, cancellationToken);
    }

    public async ValueTask DisposeAsync()
    {
        await _semaphore.WaitAsync();
        try
        {
            if (_disposed) return;
            
            _disposed = true;
            
            if (_connection != null)
            {
                await _connection.DisposeAsync();
                _connection = null;
            }
        }
        finally
        {
            _semaphore.Release();
        }
        
        _semaphore.Dispose();
    }
}