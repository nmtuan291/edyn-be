using System.ComponentModel.DataAnnotations;

namespace ChatService.ChatService.Application.DTOs;

public record MessageDto
{
    public Guid? Id { get; init; }
    public Guid? ConversationId { get; init; }
    public string? SenderId { get; init; }

    [Required]
    [StringLength(100)]
    public string ReceiverId { get; init; } = string.Empty;

    [Required]
    [StringLength(4000, MinimumLength = 1)]
    public string Content { get; init; } = string.Empty;

    public DateTime? CreatedAt { get; init; }
    public DateTime? EditedAt { get; init; }
    public bool IsDeleted { get; init; } = false;
    public bool IsRead { get; init; } = false;
    public DateTime? ReadAt { get; init; }
}