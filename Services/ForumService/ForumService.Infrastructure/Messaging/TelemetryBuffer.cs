using System.Threading.Channels;

namespace ForumService.ForumService.Infrastructure.Messaging;

public class TelemetryBuffer(BoundedChannelOptions options) : BoundedChannelBuffer<TelemetryLog>(options) { }

public record TelemetryLog(Guid UserId, Guid ForumId, Guid PostId, string IpAddress, bool IsFirstPage);