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

    public async Task<List<MessageDto>?> GetMessagesByConversationIdAsync(Guid conversationId, string userId)
    {
        var conversation = await _chatRepository.GetConversationByIdAsync(conversationId);
        if (conversation == null)
            return null;

        if (conversation.User1Id != userId && conversation.User2Id != userId)
            return null;

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
        
        return messages;
    }

    public async Task<bool> AddMessage(MessageDto message, string userId)
    {
        Message newMessage = new()
        {
            Id = Guid.NewGuid(),
            CreatedAt = DateTime.UtcNow,
            Content = message.Content,
            ConversationId = message.ConversationId ?? Guid.Empty,
            ReadAt = null,
            EditedAt = message.EditedAt,
            IsDeleted = false,
            ReceiverId = message.ReceiverId,
            SenderId = userId,
            IsRead = false,
        };

        bool isConversationExist = true;
        if (newMessage.ConversationId == Guid.Empty)
        {
            var existing = await _chatRepository.GetConversationBetweenUsersAsync(userId, message.ReceiverId);
            if (existing != null)
            {
                newMessage.ConversationId = existing.Id;
            }
            else
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
        }

        if (isConversationExist)
        {
            var isMessageAdded = await _chatRepository.InsertMessageAsync(newMessage);
            // Publish a DTO — domain Message has Conversation/Messages graph and breaks JSON serialization.
            var published = new MessageDto
            {
                Id = newMessage.Id,
                ConversationId = newMessage.ConversationId,
                SenderId = newMessage.SenderId,
                ReceiverId = newMessage.ReceiverId,
                Content = newMessage.Content,
                CreatedAt = newMessage.CreatedAt,
                EditedAt = newMessage.EditedAt,
                IsDeleted = newMessage.IsDeleted,
                IsRead = newMessage.IsRead,
                ReadAt = newMessage.ReadAt,
            };
            await _chatPublisher.SendMessageAsync(published, newMessage.SenderId, newMessage.ReceiverId);
            return isMessageAdded;
        }
        
        return false;
    }
}