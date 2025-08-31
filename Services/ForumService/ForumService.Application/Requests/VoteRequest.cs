namespace ForumService.ForumService.Application.Requests;

public class VoteRequest
{
    public Guid Id { get; set; }
    public bool IsDownvote  { get; set; } 
}