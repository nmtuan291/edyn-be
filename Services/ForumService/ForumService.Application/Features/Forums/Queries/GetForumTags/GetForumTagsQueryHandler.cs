using ForumService.ForumService.Application.DTOs;
using ForumService.ForumService.Application.Exceptions;
using ForumService.ForumService.Application.Interfaces.UnitOfWork;
using MediatR;

namespace ForumService.ForumService.Application.Features.Forums.Queries.GetForumTags;

public sealed class GetForumTagsQueryHandler : IRequestHandler<GetForumTagsQuery, List<ForumTagDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetForumTagsQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<List<ForumTagDto>> Handle(GetForumTagsQuery request, CancellationToken cancellationToken)
    {
        var forum = await _unitOfWork.ForumRepo.GetForumByIdAsync(request.ForumId, cancellationToken);
        if (forum == null)
            throw new ForumNotFoundException(request.ForumId);

        return await _unitOfWork.ForumRepo.GetForumTagCatalogAsync(request.ForumId, cancellationToken);
    }
}
