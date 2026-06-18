using AutoMapper;
using ForumService.ForumService.Application.DTOs;
using ForumService.ForumService.Application.Interfaces.UnitOfWork;
using MediatR;

namespace ForumService.ForumService.Application.Features.Threads.Commands.VotePoll;

public sealed class VotePollCommandHandler : IRequestHandler<VotePollCommand, ForumThreadDto?>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public VotePollCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<ForumThreadDto?> Handle(VotePollCommand request, CancellationToken cancellationToken)
    {
        var thread = await _unitOfWork.ThreadRepo.VotePollAsync(
            request.UserId,
            request.ThreadId,
            request.PollContent);

        if (thread == null)
            return null;

        await _unitOfWork.CommitAsync();

        var dto = _mapper.Map<ForumThreadDto>(thread);
        dto.UserPollVote = thread.PollVotes?
            .FirstOrDefault(v => v.UserId == request.UserId)
            ?.PollContent;

        return dto;
    }
}
