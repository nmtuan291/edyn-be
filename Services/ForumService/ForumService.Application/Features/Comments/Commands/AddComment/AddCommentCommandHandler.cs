using ForumService.ForumService.Application.DTOs;
using ForumService.ForumService.Application.Enums;
using ForumService.ForumService.Application.Interfaces.Services;
using ForumService.ForumService.Application.Interfaces.UnitOfWork;
using ForumService.ForumService.Application.Requests;
using ForumService.ForumService.Domain.Entities;
using ForumService.ForumService.Domain.ValueObjects;
using MediatR;

namespace ForumService.ForumService.Application.Features.Comments.Commands.AddComment;

public sealed class AddCommentCommandHandler : IRequestHandler<AddCommentCommand, AddCommentResult>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPermissionService _permissionService;
    private readonly ICommentNotificationSender _commentNotificationSender;

    public AddCommentCommandHandler(
        IUnitOfWork unitOfWork,
        IPermissionService permissionService,
        ICommentNotificationSender commentNotificationSender)
    {
        _unitOfWork = unitOfWork;
        _permissionService = permissionService;
        _commentNotificationSender = commentNotificationSender;
    }

    public async Task<AddCommentResult> Handle(AddCommentCommand request, CancellationToken cancellationToken)
    {
        if (request.Comment.ThreadId == Guid.Empty)
            throw new ArgumentException("Thread Id cannot be empty", nameof(request.Comment));

        var thread = await _unitOfWork.ThreadRepo.GetThreadByIdAsync(
            request.Comment.ThreadId,
            request.UserId,
            cancellationToken);

        if (thread == null)
            return AddCommentResult.ThreadNotFound;

        var canComment = await _permissionService.HasPermissionAsync(
            thread.ForumId,
            request.UserId,
            ForumPermissionType.CreateComment,
            cancellationToken);

        if (!canComment)
            return AddCommentResult.Forbidden;

        if (request.Comment.ParentId is { } parentId && parentId != Guid.Empty)
        {
            var parent = await _unitOfWork.CommentRepo.GetCommentByIdAsync(parentId, cancellationToken);
            if (parent == null || parent.ThreadId != request.Comment.ThreadId)
                throw new ArgumentException("Parent comment does not exist or is not on this thread.");
        }

        var comment = new Comment(new CommentCreation(
            request.Comment.ThreadId,
            request.UserId,
            request.Username,
            request.Comment.Content,
            request.Comment.ParentId));

        await _unitOfWork.CommentRepo.InsertCommentAsync(comment);

        var parentComment = comment.ParentId is { } parentCommentId && parentCommentId != Guid.Empty
            ? await _unitOfWork.CommentRepo.GetParentCommentAsync(parentCommentId, cancellationToken)
            : null;

        await _commentNotificationSender.SendNotification(new CommentNotificationMessage(
            parentComment?.OwnerId.ToString() ?? "",
            request.Username,
            parentComment?.Content ?? "",
            parentComment?.ThreadId.ToString() ?? ""));

        await _unitOfWork.CommitAsync();
        return AddCommentResult.Created;
    }
}
