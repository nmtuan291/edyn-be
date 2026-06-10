using AutoMapper;
using ForumService.ForumService.Application.DTOs;
using ForumService.ForumService.Application.Enums;
using ForumService.ForumService.Application.Interfaces.UnitOfWork;
using MediatR;

namespace ForumService.ForumService.Application.Features.Threads.Queries.GetComments;

public sealed class GetCommentsQueryHandler : IRequestHandler<GetCommentsQuery, List<CommentDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetCommentsQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<List<CommentDto>> Handle(GetCommentsQuery request, CancellationToken cancellationToken)
    {
        if (request.ThreadId == Guid.Empty)
            throw new ArgumentNullException(nameof(request.ThreadId));

        var comments = await _unitOfWork.CommentRepo.GetCommentByThreadIdAsync(
            request.ThreadId,
            cancellationToken);

        var votedComments = string.IsNullOrEmpty(request.UserId)
            ? new Dictionary<Guid, bool>()
            : await _unitOfWork.VoteRepo.GetVotedCommentsAsync(Guid.Parse(request.UserId), request.ThreadId);

        return _mapper.Map<List<CommentDto>>(comments)
            .Select(dto =>
            {
                if (!string.IsNullOrEmpty(request.UserId))
                {
                    dto.Vote = votedComments.TryGetValue(dto.Id!.Value, out var isDownVote)
                        ? isDownVote ? VoteStatus.DownVote : VoteStatus.UpVote
                        : VoteStatus.NoVote;
                }
                else
                {
                    dto.Vote = VoteStatus.NoVote;
                }

                return dto;
            })
            .ToList();
    }
}
