using System.ComponentModel.DataAnnotations;

namespace ForumService.ForumService.Domain.Entities;

public class PollVote
{
    public Guid UserId { get; set; }
    public Guid ThreadId { get; set; }
    public string PollContent { get; set; }
}
