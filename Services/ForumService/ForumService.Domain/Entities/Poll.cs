using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ForumService.ForumService.Domain.Entities;

public class Poll
{
    [Required]
    public Guid ThreadId { get; set; }
    
    [Required]
    public string PollContent { get; set; }
    
    [Required]
    public int VoteCount { get; set; }
    
    [ForeignKey("ThreadId")]
    public ForumThread Thread { get; set; }
}