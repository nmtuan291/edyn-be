namespace ChatService.ChatService.Application.DTOs;

public class MessageDto
{
    public Guid? Id { get; set; }
    public Guid? ConversationId { get; set; }
    public string? SenderId { get; set; }
    public string ReceiverId { get; set; }
    public string Content { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? EditedAt { get; set; }
    public bool IsDeleted { get; set; } = false;
    public bool IsRead { get; set; } = false;
    public DateTime? ReadAt { get; set; }
}