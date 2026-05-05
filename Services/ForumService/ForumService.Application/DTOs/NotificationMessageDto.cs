namespace ForumService.ForumService.Application.DTOs;

public record NotificationMessageDto
{
    public string UserId { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;
    public DateTime CreatedOn { get; init; }
}