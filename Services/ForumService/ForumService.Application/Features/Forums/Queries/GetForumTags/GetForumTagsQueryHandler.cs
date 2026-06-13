using ForumService.ForumService.Application.DTOs;
using ForumService.ForumService.Application.Exceptions;
using ForumService.ForumService.Application.Interfaces.Repositories;
using MediatR;

namespace ForumService.ForumService.Application.Features.Forums.Queries.GetForumTags;

public sealed class GetForumTagsQueryHandler : IRequestHandler<GetForumTagsQuery, List<ForumTagDto>>
{
    private readonly IForumQueryRepository _forumRepository;

    public GetForumTagsQueryHandler(IForumQueryRepository forumRepository)
    {
        _forumRepository = forumRepository;
    }

    public async Task<List<ForumTagDto>> Handle(GetForumTagsQuery request, CancellationToken cancellationToken)
    {
        var forum = await _forumRepository.GetForumByIdAsync(request.ForumId, cancellationToken);
        if (forum == null)
            throw new ForumNotFoundException(request.ForumId);

        return await _forumRepository.GetForumTagCatalogAsync(request.ForumId, cancellationToken);
    }
}
