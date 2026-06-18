using AutoMapper;
using ForumService.ForumService.Application.DTOs;
using ForumService.ForumService.Application.Interfaces.UnitOfWork;
using MediatR;

namespace ForumService.ForumService.Application.Features.Comments.Commands.EditComment;

public sealed class EditCommentCommandHandler : IRequestHandler<EditCommentCommand, CommentDto?>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public EditCommentCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<CommentDto?> Handle(EditCommentCommand request, CancellationToken cancellationToken)
    {
        var comment = await _unitOfWork.CommentRepo.GetCommentByIdAsync(request.CommentId, cancellationToken);
        if (comment == null)
            return null;

        if (comment.OwnerId != request.UserId)
            throw new UnauthorizedAccessException("You can only edit your own comments.");

        comment.Content = request.Request.Content;
        comment.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.CommentRepo.UpdateCommentAsync(comment);
        await _unitOfWork.CommitAsync();

        return _mapper.Map<CommentDto>(comment);
    }
}
