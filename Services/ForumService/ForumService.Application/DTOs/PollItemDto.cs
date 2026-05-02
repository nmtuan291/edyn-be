namespace ForumService.ForumService.Application.DTOs;

public class PollItemDto
{
    public required string PollContent { get; set; }
    public required int VoteCount { get; set; }
}