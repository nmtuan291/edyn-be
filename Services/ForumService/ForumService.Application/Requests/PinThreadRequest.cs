namespace ForumService.ForumService.Application.Requests;

public record PinThreadRequest
{
    public bool IsPinned { get; init; }
}
