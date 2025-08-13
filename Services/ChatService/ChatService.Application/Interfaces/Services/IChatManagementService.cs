using ChatService.ChatService.Application.DTOs;
using ChatService.ChatService.Domain.Entities;

namespace ChatService.ChatService.Application.Interfaces.Services;

public interface IChatManagementService
{
    Task<List<Conversation>> GetConversationsByUserId(string userId);
    Task<bool> AddMessage(MessageDto message, string userId);
    Task<List<MessageDto>> GetMessagesByConversationIdAsync(Guid conversationId);
}