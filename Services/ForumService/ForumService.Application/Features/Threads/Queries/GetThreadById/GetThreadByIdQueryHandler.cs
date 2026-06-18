using AutoMapper;
using ForumService.ForumService.Application.DTOs;
using ForumService.ForumService.Application.Interfaces.Repositories;
using MediatR;

namespace ForumService.ForumService.Application.Features.Threads.Queries.GetThreadById;

public sealed class GetThreadByIdQueryHandler : IRequestHandler<GetThreadByIdQuery, ForumThreadDto?>
{
    private readonly IThreadQueryRepository _threadRepository;
    private readonly IMapper _mapper;

    public GetThreadByIdQueryHandler(IThreadQueryRepository threadRepository, IMapper mapper)
    {
        _threadRepository = threadRepository;
        _mapper = mapper;
    }

    public async Task<ForumThreadDto?> Handle(GetThreadByIdQuery request, CancellationToken cancellationToken)
    {
        var parsedUserId = Guid.TryParse(request.UserId, out var userIdGuid) ? userIdGuid : Guid.Empty;

        var thread = await _threadRepository.GetThreadByIdAsync(
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
