namespace NotificationService.Entities;

public class Notification
{
    public required Guid Id { get; set; }
    public required Guid RecipientId { get; set; }
    public required DateTime CreatedAt { get; set; }
    public required string Message { get; set; }
    public bool IsRead { get; set; } = false;
}