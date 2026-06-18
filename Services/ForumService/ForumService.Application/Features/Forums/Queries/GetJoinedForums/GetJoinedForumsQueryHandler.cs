using ForumService.ForumService.Application.DTOs;
using ForumService.ForumService.Application.Interfaces.Repositories;
using MediatR;

namespace ForumService.ForumService.Application.Features.Forums.Queries.GetJoinedForums;

public sealed class GetJoinedForumsQueryHandler : IRequestHandler<GetJoinedForumsQuery, List<ForumUserDto>>
{
    private readonly IForumQueryRepository _forumRepository;

    public GetJoinedForumsQueryHandler(IForumQueryRepository forumRepository)
    {
        _forumRepository = forumRepository;
    }

    public async Task<List<ForumUserDto>> Handle(GetJoinedForumsQuery request, CancellationToken cancellationToken)
    {
        return (await _forumRepository.GetJoinedForumsByUserIdAsync(request.UserId, cancellationToken))
            .Select(f => new ForumUserDto
            {
                ForumId = f.ForumId,
                Name = f.Forum.Name,
                ForumImage = f.Forum.ForumImage,
                Role = f.Role,
            })
            .ToList();
    }
}
