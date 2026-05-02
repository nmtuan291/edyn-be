using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ForumService.ForumService.Domain.Entities;

public class Poll
{
    public Guid ThreadId { get; set; }
    public string PollContent { get; set; }
    public int VoteCount { get; set; }
    
}