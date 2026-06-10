using AutoMapper;
using ForumService.ForumService.Application.DTOs;
using ForumService.ForumService.Application.Interfaces.UnitOfWork;
using ForumService.ForumService.Application.Requests;
using MediatR;

namespace ForumService.ForumService.Application.Features.Comments.Commands.VoteComment;

public sealed class VoteCommentCommandHandler : IRequestHandler<VoteCommentCommand, CommentDto?>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public VoteCommentCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<CommentDto?> Handle(VoteCommentCommand request, CancellationToken cancellationToken)
    {
        var comment = await _unitOfWork.CommentRepo.GetCommentByIdAsync(request.CommentId, cancellationToken);
        if (comment == null)
            return null;

        var isVoteExists = comment.Vote(request.UserId, request.IsDownVote);
        await _unitOfWork.CommentRepo.UpdateCommentAsync(comment);

        if (isVoteExists)
        {
            await _unitOfWork.VoteRepo.UpdateCommentVoteRedisAsync(
                new CommentVoteRedisUpdate(request.UserId, request.CommentId, comment.ThreadId, request.IsDownVote));
        }
        else
        {
            await _unitOfWork.VoteRepo.RemoveCommentVoteRedisAsync(
                request.UserId,
                request.CommentId,
                comment.ThreadId);
        }

        await _unitOfWork.CommitAsync();
        return _mapper.Map<CommentDto>(comment);
    }
}
