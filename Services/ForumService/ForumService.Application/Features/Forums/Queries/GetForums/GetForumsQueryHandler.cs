using AutoMapper;
using ForumService.ForumService.Application.DTOs;
using ForumService.ForumService.Application.Interfaces.Repositories;
using MediatR;

namespace ForumService.ForumService.Application.Features.Forums.Queries.GetForums;

public sealed class GetForumsQueryHandler : IRequestHandler<GetForumsQuery, List<ForumDto>>
{
    private readonly IForumQueryRepository _forumRepository;
    private readonly IMapper _mapper;

    public GetForumsQueryHandler(IForumQueryRepository forumRepository, IMapper mapper)
    {
        _forumRepository = forumRepository;
        _mapper = mapper;
    }

    public async Task<List<ForumDto>> Handle(GetForumsQuery request, CancellationToken cancellationToken)
    {
        var forums = await _forumRepository.GetForumsAsync(cancellationToken);
        return _mapper.Map<List<ForumDto>>(forums);
    }
}
