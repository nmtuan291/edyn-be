using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ChatService.ChatService.Domain.Entities;

public class Message
{
    [Key]
    public Guid Id { get; set; }
    
    [Required]
    public Guid ConversationId { get; set; }
    
    [ForeignKey("ConversationId")]
    public Conversation Conversation { get; set; }
    
    [Required]
    [StringLength(100)]
    public string SenderId { get; set; }
    
    [Required]
    [StringLength(100)]
    public string ReceiverId { get; set; }
    
    [Required]
    [StringLength(4000)]
    public string Content { get; set; }
    
    [Required]
    public DateTime CreatedAt { get; set; }
    
    public DateTime? EditedAt { get; set; }
    
    public bool IsDeleted { get; set; } = false;
    
    public bool IsRead { get; set; } = false;
    
    public DateTime? ReadAt { get; set; }
}