using AutoMapper;
using ForumService.ForumService.Application.DTOs;
using ForumService.ForumService.Application.Interfaces.Repositories;
using MediatR;

namespace ForumService.ForumService.Application.Features.Forums.Queries.GetForumByName;

public sealed class GetForumByNameQueryHandler : IRequestHandler<GetForumByNameQuery, ForumDto?>
{
    private readonly IForumQueryRepository _forumRepository;
    private readonly IMapper _mapper;

    public GetForumByNameQueryHandler(IForumQueryRepository forumRepository, IMapper mapper)
    {
        _forumRepository = forumRepository;
        _mapper = mapper;
    }

    public async Task<ForumDto?> Handle(GetForumByNameQuery request, CancellationToken cancellationToken)
    {
        var forum = await _forumRepository.GetForumByNameAsync(
            request.UserId,
            request.ForumName,
            cancellationToken);

        return forum == null ? null : _mapper.Map<ForumDto>(forum);
    }
}
