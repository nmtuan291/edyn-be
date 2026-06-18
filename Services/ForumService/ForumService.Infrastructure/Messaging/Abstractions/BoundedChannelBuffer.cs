using System.Threading.Channels;

namespace ForumService.ForumService.Infrastructure.Messaging;

public abstract class BoundedChannelBuffer<T>(BoundedChannelOptions options)
{
    private readonly Channel<T>  _channel = Channel.CreateBounded<T>(options);
    
    public ChannelReader<T> Reader => _channel.Reader;
    public ChannelWriter<T> Writer => _channel.Writer;
}