using AutoMapper;
using ForumService.ForumService.Application.DTOs;
using ForumService.ForumService.Application.Interfaces.UnitOfWork;
using MediatR;

namespace ForumService.ForumService.Application.Features.Threads.Queries.GetThreadById;

public sealed class GetThreadByIdQueryHandler : IRequestHandler<GetThreadByIdQuery, ForumThreadDto?>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetThreadByIdQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<ForumThreadDto?> Handle(GetThreadByIdQuery request, CancellationToken cancellationToken)
    {
        var parsedUserId = Guid.TryParse(request.UserId, out var userIdGuid) ? userIdGuid : Guid.Empty;

        var thread = await _unitOfWork.ThreadRepo.GetThreadByIdAsync(
            request.ThreadId,
            parsedUserId,
            cancellationToken);

        if (thread == null)
            return null;

        var dto = _mapper.Map<ForumThreadDto>(thread);
        if (Guid.TryParse(request.UserId, out var currentUserId))
        {
            dto.UserPollVote = thread.PollVotes?
                .FirstOrDefault(v => v.UserId == currentUserId)
                ?.PollContent;
        }

        return dto;
    }
}
