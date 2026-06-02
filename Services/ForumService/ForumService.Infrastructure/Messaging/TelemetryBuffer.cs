using System.Threading.Channels;

namespace ForumService.ForumService.Infrastructure.Messaging;

public class TelemetryBuffer(BoundedChannelOptions options) : BoundedChannelBuffer<TelemetryLog>(options) { }

public record TelemetryLog(string Placeholder);