using AutoMapper;
using ForumService.ForumService.Application.DTOs;
using ForumService.ForumService.Application.Interfaces.Repositories;
using MediatR;

namespace ForumService.ForumService.Application.Features.Forums.Queries.GetRecentForums;

public sealed class GetRecentForumsQueryHandler : IRequestHandler<GetRecentForumsQuery, List<ForumDto>>
{
    private readonly IForumQueryRepository _forumRepository;
    private readonly IMapper _mapper;

    public GetRecentForumsQueryHandler(IForumQueryRepository forumRepository, IMapper mapper)
    {
        _forumRepository = forumRepository;
        _mapper = mapper;
    }

    public async Task<List<ForumDto>> Handle(GetRecentForumsQuery request, CancellationToken cancellationToken)
    {
        var forums = await _forumRepository.GetRecentVisitForumsAsync(request.UserId, cancellationToken);
        return _mapper.Map<List<ForumDto>>(forums);
    }
}
