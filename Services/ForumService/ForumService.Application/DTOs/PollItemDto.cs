namespace ForumService.ForumService.Application.DTOs;

public record PollItemDto
{
    public required string PollContent { get; init; }
    public required int VoteCount { get; init; }
}