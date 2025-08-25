using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ForumService.ForumService.Infrastructure.Models;

public class PollEf
{
    [Required]
    public Guid ThreadId { get; set; }
    
    [Required]
    public string PollContent { get; set; }
    
    [Required]
    public int VoteCount { get; set; }
    
    [ForeignKey("ThreadId")]
    public ForumThreadEf ThreadEf { get; set; }
}