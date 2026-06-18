using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ForumService.ForumService.Infrastructure.Models;

public class PollVoteEf
{
    [Required]
    public Guid UserId { get; set; }
    
    [Required]
    public Guid ThreadId { get; set; }
    
    [Required]
    public string PollContent { get; set; }
    
    [ForeignKey("ThreadId")]
    public ForumThreadEf ThreadEf { get; set; }
}
