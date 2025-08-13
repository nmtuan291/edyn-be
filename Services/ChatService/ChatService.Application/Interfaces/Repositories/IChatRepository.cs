using ChatService.ChatService.Domain.Entities;

namespace ChatService.ChatService.Application.Interfaces.Repositories;

public interface IChatRepository
{
    Task<List<Conversation>> GetConversationsByUserIdAsync(string userId);
    Task<bool> InsertConversationAsync(Conversation conversation);
    Task<bool> InsertMessageAsync(Message message);
    Task<List<Message>> GetMessagesByConversationIdAsync(Guid conversationId);
}