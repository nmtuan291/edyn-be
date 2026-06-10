using ForumService.ForumService.Application.Interfaces.UnitOfWork;
using MediatR;

namespace ForumService.ForumService.Application.Features.Comments.Commands.DeleteComment;

public sealed class DeleteCommentCommandHandler : IRequestHandler<DeleteCommentCommand>
{
    private readonly IUnitOfWork _unitOfWork;

    public DeleteCommentCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(DeleteCommentCommand request, CancellationToken cancellationToken)
    {
        var comment = await _unitOfWork.CommentRepo.GetCommentByIdAsync(request.CommentId, cancellationToken);
        if (comment == null)
            throw new KeyNotFoundException("Comment not found.");

        if (comment.OwnerId != request.UserId)
            throw new UnauthorizedAccessException("You can only delete your own comments.");

        await _unitOfWork.CommentRepo.DeleteCommentById(request.CommentId);
        await _unitOfWork.CommitAsync();
    }
}
