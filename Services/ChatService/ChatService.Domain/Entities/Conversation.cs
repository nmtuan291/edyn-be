using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ChatService.ChatService.Domain.Entities;

public class Conversation
{
    [Key]
    public Guid Id { get; set; }
    
    [Required]
    [StringLength(100)]
    public string User1Id { get; set; }
    
    [Required]
    [StringLength(100)]
    public string User2Id { get; set; }
    
    [Required]
    public DateTime CreatedAt { get; set; }
    
    public DateTime? LastMessageAt { get; set; }
    
    public Guid? LastMessageId { get; set; }
    
    [ForeignKey("LastMessageId")]
    public Message LastMessage { get; set; }
    
    public ICollection<Message> Messages { get; set; }
}