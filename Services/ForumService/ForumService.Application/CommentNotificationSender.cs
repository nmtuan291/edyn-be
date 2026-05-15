using ForumService.ForumService.Application.DTOs;
using ForumService.ForumService.Application.Interfaces.Services;
using ForumService.ForumService.Application.Requests;
using ForumService.ForumService.Infrastructure.Messaging;
using ForumService.ForumService.Application.Interfaces.UnitOfWork;

namespace ForumService.ForumService.Application;

public class CommentNotificationSender : ICommentNotificationSender
{
    private readonly IUnitOfWork _unitOfWork;

    public CommentNotificationSender(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task SendNotification(CommentNotificationMessage message)
    {
        var payload = new NotificationMessageDto()
        {
            UserId = message.UserId,
            Message = $"Người dùng {message.UserName} đã trả lời bình luận của bạn",
            CreatedOn = DateTime.UtcNow,
        };

        await _unitOfWork.OutboxRepo.AddAsync(
            aggregateType: "Notification",
            aggregateId: message.UserId,
            eventType: "CommentNotification",
            payload: payload
        );
    }
}