using ChatService.ChatService.Domain.Entities;

namespace ChatService.ChatService.Application.Interfaces.Repositories;

public interface IChatRepository
{
    Task<List<Conversation>> GetConversationsByUserIdAsync(string userId);
    Task<bool> InsertConversationAsync(Conversation conversation);
    Task<bool> InsertMessageAsync(Message message);
    Task<Conversation?> GetConversationByIdAsync(Guid conversationId);
    /// <summary>Finds a conversation for this pair of users, regardless of User1/User2 order.</summary>
    Task<Conversation?> GetConversationBetweenUsersAsync(string userId, string otherUserId);
    Task<List<Message>> GetMessagesByConversationIdAsync(Guid conversationId);
}