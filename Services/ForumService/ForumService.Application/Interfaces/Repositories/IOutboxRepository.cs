namespace ForumService.ForumService.Application.Interfaces.Repositories;

public interface IOutboxRepository
{
    Task AddAsync(string aggregateType, string aggregateId, string eventType, object payload);
}
