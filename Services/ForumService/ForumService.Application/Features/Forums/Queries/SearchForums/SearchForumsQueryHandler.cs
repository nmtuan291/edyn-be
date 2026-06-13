using AutoMapper;
using ForumService.ForumService.Application.DTOs;
using ForumService.ForumService.Application.Interfaces.Repositories;
using MediatR;

namespace ForumService.ForumService.Application.Features.Forums.Queries.SearchForums;

public sealed class SearchForumsQueryHandler : IRequestHandler<SearchForumsQuery, List<ForumDto>>
{
    private readonly IForumQueryRepository _forumRepository;
    private readonly IMapper _mapper;

    public SearchForumsQueryHandler(IForumQueryRepository forumRepository, IMapper mapper)
    {
        _forumRepository = forumRepository;
        _mapper = mapper;
    }

    public async Task<List<ForumDto>> Handle(SearchForumsQuery request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Query))
            return [];

        var forums = await _forumRepository.SearchForumsAsync(request.Query, cancellationToken);
        return _mapper.Map<List<ForumDto>>(forums);
    }
}
