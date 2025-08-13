using RabbitMQ.Client;

namespace ChatService.ChatService.Infrastructure.Messaging;

public class RabbitMqConnectionFactory: IAsyncDisposable
{
    private IConnection? _connection;
    private readonly IConfiguration _configuration;
    private readonly SemaphoreSlim _semaphore= new(1, 1);
    private bool _disposed = false;

    public RabbitMqConnectionFactory(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task CreateConnectionAsync()
    {
        if (_connection == null)
        {
            await _semaphore.WaitAsync();
            try
            {
                if (_connection == null)
                {
                    var factory = new ConnectionFactory
                    {
                        HostName = _configuration["RabbitMQ:HostName"] ?? "localhost",
                        Port = _configuration.GetValue<int>("RabbitMQ:Port", 5672),
                        UserName = _configuration["RabbitMQ:UserName"] ?? "guest",
                        Password = _configuration["RabbitMQ:Password"] ?? "guest"
                    };

                    _connection = await factory.CreateConnectionAsync();
                }
            }
            finally
            {
                _semaphore.Release();
            }
         }
    }

    public async Task<IChannel> CreateChannelAsync()
    {
        if (_connection == null)
            await CreateConnectionAsync();
        
        await _semaphore.WaitAsync();
        try
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(RabbitMqConnectionFactory));
            
            return await _connection.CreateChannelAsync();
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_connection != null && !_disposed)
        {
            await _semaphore.WaitAsync();;
            try
            {
                if (_disposed)
                {
                    await _connection.DisposeAsync();
                    _disposed = true;
                    _connection = null;
                }
            }
            finally
            {
                _semaphore.Release();
            }
        }
        _semaphore.Dispose();
    }
}