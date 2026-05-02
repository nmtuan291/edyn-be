using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NotificationService.Interface;

namespace NotificationService.Controllers;

[Route("api/[controller]")]
[ApiController]
public class NotificationController : ControllerBase
{
    private readonly INotificationMessageService _notificationMessageService;
    
    public NotificationController(INotificationMessageService notificationMessageService)
    {
        _notificationMessageService = notificationMessageService;
    }

    [Authorize]
    [HttpGet]
    public async Task<IActionResult> GetNotifications()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();
        var notifications = await _notificationMessageService.GetUserNotification(userId, DateTime.UtcNow.AddDays(-10), DateTime.UtcNow);
        return Ok(notifications);
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> UpdateNotification(Guid notificationId)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var result = await _notificationMessageService.MarkAsRead(notificationId, Guid.Parse(userId));
        if (!result)
            return NotFound();

        return Ok();
    }
}
