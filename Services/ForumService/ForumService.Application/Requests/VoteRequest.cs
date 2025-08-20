namespace ForumService.ForumService.Application.Requests;

public class VoteRequest
{
    public Guid ThreadId { get; set; }
    public bool IsDownvote  { get; set; } 
}