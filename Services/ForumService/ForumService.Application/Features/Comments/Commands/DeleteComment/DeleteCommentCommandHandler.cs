using ForumService.ForumService.Application.Enums;
using ForumService.ForumService.Application.Interfaces.Services;
using ForumService.ForumService.Application.Interfaces.UnitOfWork;
using MediatR;

namespace ForumService.ForumService.Application.Features.Comments.Commands.DeleteComment;

public sealed class DeleteCommentCommandHandler : IRequestHandler<DeleteCommentCommand>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPermissionService _permissionService;

    public DeleteCommentCommandHandler(IUnitOfWork unitOfWork, IPermissionService permissionService)
    {
        _unitOfWork = unitOfWork;
        _permissionService = permissionService;
    }

    public async Task Handle(DeleteCommentCommand request, CancellationToken cancellationToken)
    {
        var comment = await _unitOfWork.CommentRepo.GetCommentByIdAsync(request.CommentId, cancellationToken);
        if (comment == null)
            throw new KeyNotFoundException("Comment not found.");

        // The owner can always delete; otherwise a moderator with DeleteComment permission may.
        if (comment.OwnerId != request.UserId)
        {
            var thread = await _unitOfWork.ThreadRepo.GetThreadByIdAsync(comment.ThreadId, cancellationToken: cancellationToken);
            var canModerate = thread != null && await _permissionService.HasPermissionAsync(
                thread.ForumId,
                request.UserId,
                ForumPermissionType.DeleteComment,
                cancellationToken);

            if (!canModerate)
                throw new UnauthorizedAccessException("You do not have permission to delete this comment.");
        }

        await _unitOfWork.CommentRepo.DeleteCommentById(request.CommentId);
        await _unitOfWork.CommitAsync();
    }
}
