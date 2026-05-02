using ChatService.ChatService.Application.Interfaces.Repositories;
using ChatService.ChatService.Domain.Entities;
using ChatService.ChatService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ChatService.ChatService.Infrastructure.Repositories;

public class ChatRepository: IChatRepository
{
    private readonly ChatDbContext _chatDbContext;

    public ChatRepository(ChatDbContext chatDbContext)
    {
        _chatDbContext = chatDbContext;
    }

    public async Task<List<Conversation>> GetConversationsByUserIdAsync(string userId)
    {
        return await _chatDbContext.Conversations
            .Where(c => c.User1Id == userId || c.User2Id == userId)
            .Include(c => c.LastMessage)
            .ToListAsync();
    }

    public async Task<Conversation?> GetConversationByIdAsync(Guid conversationId)
    {
        return await _chatDbContext.Conversations.FindAsync(conversationId);
    }

    public async Task<Conversation?> GetConversationBetweenUsersAsync(string userId, string otherUserId)
    {
        return await _chatDbContext.Conversations
            .AsNoTracking()
            .FirstOrDefaultAsync(c =>
                (c.User1Id == userId && c.User2Id == otherUserId) ||
                (c.User1Id == otherUserId && c.User2Id == userId));
    }

    public async Task<List<Message>> GetMessagesByConversationIdAsync(Guid conversationId)
    {
        return await _chatDbContext.Messages
            .Where(m => m.ConversationId == conversationId)
            .OrderBy(m => m.CreatedAt)
            .ToListAsync();
    }
    public async Task<bool> InsertConversationAsync(Conversation conversation)
    {
        var success = await _chatDbContext.Conversations.AddAsync(conversation);
        if (success.State == EntityState.Added)
        {
            await _chatDbContext.SaveChangesAsync();
            return true;
        }
        
        return false;
    }

    public async Task<bool> InsertMessageAsync(Message message)
    {
        var success = await _chatDbContext.Messages.AddAsync(message);
        if (success.State == EntityState.Added)
        {
            await _chatDbContext.SaveChangesAsync();
            return true;
        }
        
        return false;
    }
}