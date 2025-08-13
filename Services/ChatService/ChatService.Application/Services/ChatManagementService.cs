using ChatService.ChatService.Application.DTOs;
using ChatService.ChatService.Application.Interfaces.Messaging;
using ChatService.ChatService.Application.Interfaces.Repositories;
using ChatService.ChatService.Application.Interfaces.Services;
using ChatService.ChatService.Domain.Entities;

namespace ChatService.ChatService.Application.Services;

public class ChatManagementService: IChatManagementService
{
    private readonly IChatRepository _chatRepository;
    private readonly IChatPublisher _chatPublisher;

    public ChatManagementService(IChatRepository chatRepository, IChatPublisher chatPublisher)
    {
        _chatRepository = chatRepository;
        _chatPublisher = chatPublisher;
    }

    public async Task<List<Conversation>> GetConversationsByUserId(string userId)
    {
        return await _chatRepository.GetConversationsByUserIdAsync(userId);
    }

    public async Task<List<MessageDto>> GetMessagesByConversationIdAsync(Guid conversationId)
    {
        var messages = (await _chatRepository.GetMessagesByConversationIdAsync(conversationId))
            .Select(m => new MessageDto
            {
                Id = m.Id,
                ConversationId = m.ConversationId,
                SenderId = m.SenderId,
                ReceiverId = m.ReceiverId,
                Content = m.Content,
                CreatedAt = m.CreatedAt,
                EditedAt = m.EditedAt,
                IsDeleted = m.IsDeleted,
                IsRead = m.IsRead,
                ReadAt = m.ReadAt
            })
            .ToList();
        
        return  messages;
    }

    public async Task<bool> AddMessage(MessageDto message, string userId)
    {
        Message newMessage = new()
        {
            Id = Guid.NewGuid(),
            CreatedAt = DateTime.UtcNow,
            Content = message.Content,
            ConversationId = message.ConversationId ?? Guid.Empty,
            ReadAt = DateTime.UtcNow,
            EditedAt = message.EditedAt,
            IsDeleted = false,
            ReceiverId = message.ReceiverId,
            SenderId = userId,
            IsRead = message.IsRead,
        };

        bool isConversationExist = true;
        if (newMessage.ConversationId == Guid.Empty)
        {
            Conversation newConversation = new()
            {
                Id = Guid.NewGuid(),
                CreatedAt = DateTime.UtcNow,
                User1Id = userId,
                User2Id = message.ReceiverId,
                LastMessageId = null,
                LastMessageAt = DateTime.UtcNow,
            };
            isConversationExist = await _chatRepository.InsertConversationAsync(newConversation);
            newMessage.ConversationId = newConversation.Id;
        }

        if (isConversationExist)
        {
            var isMessageAdded = await _chatRepository.InsertMessageAsync(newMessage);
            await _chatPublisher.SendMessageAsync(newMessage, newMessage.SenderId, newMessage.ReceiverId);
            return isMessageAdded;
        }
        
        return false;
    }
}