using System.Text.Json;
using ForumService.ForumService.Application.Interfaces.Repositories;
using ForumService.ForumService.Infrastructure.Data;
using ForumService.ForumService.Infrastructure.Models;

namespace ForumService.ForumService.Infrastructure.Repositories;

public class OutboxRepository : IOutboxRepository
{
    private readonly ForumDbContext _context;

    public OutboxRepository(ForumDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(string aggregateType, string aggregateId, string eventType, object payload)
    {
        var message = new OutboxMessageEf
        {
            Id = Guid.NewGuid(),
            AggregateType = aggregateType,
            AggregateId = aggregateId,
            EventType = eventType,
            Payload = JsonSerializer.Serialize(payload),
            CreatedAt = DateTime.UtcNow,
        };
        await _context.OutboxMessages.AddAsync(message);
    }
}
