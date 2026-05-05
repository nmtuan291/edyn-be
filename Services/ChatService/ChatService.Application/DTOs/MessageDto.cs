namespace ChatService.ChatService.Application.DTOs;

public record MessageDto
{
    public Guid? Id { get; init; }
    public Guid? ConversationId { get; init; }
    public string? SenderId { get; init; }
    public string ReceiverId { get; init; } = string.Empty;
    public string Content { get; init; } = string.Empty;
    public DateTime? CreatedAt { get; init; }
    public DateTime? EditedAt { get; init; }
    public bool IsDeleted { get; init; } = false;
    public bool IsRead { get; init; } = false;
    public DateTime? ReadAt { get; init; }
}