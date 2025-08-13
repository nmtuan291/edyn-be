namespace ForumService.ForumService.Application.DTOs;

public class NotificationMessageDto
{
    public string UserId { get; set; }
    public string Message { get; set; }
    public DateTime CreatedOn { get; set; }
}