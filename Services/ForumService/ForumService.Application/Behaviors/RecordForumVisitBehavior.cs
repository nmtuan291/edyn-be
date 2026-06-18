using ForumService.ForumService.Application.DTOs;
using ForumService.ForumService.Application.Features.Forums.Queries.GetForumByName;
using ForumService.ForumService.Application.Features.Threads.Queries.GetThreadById;
using ForumService.ForumService.Application.Interfaces.Repositories;
using MediatR;

namespace ForumService.ForumService.Application.Behaviors;

public sealed class RecordForumVisitBehavior<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly IForumRepository _forumRepository;

    public RecordForumVisitBehavior(IForumRepository forumRepository)
    {
        _forumRepository = forumRepository;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var response = await next();

        var (userId, forumId) = ExtractVisitInfo(request, response);

        if (userId != Guid.Empty && forumId != Guid.Empty)
            _ = _forumRepository.WriteVisitAsync(userId, forumId, CancellationToken.None);

        return response;
    }

    private static (Guid UserId, Guid ForumId) ExtractVisitInfo(TRequest request, TResponse response)
    {
        switch (request)
        {
            case GetForumByNameQuery forumQuery
                when response is ForumDto { Id: not null } forumDto
                && forumQuery.UserId != Guid.Empty:
                return (forumQuery.UserId, forumDto.Id!.Value);

            case GetThreadByIdQuery threadQuery
                when response is ForumThreadDto threadDto
                && Guid.TryParse(threadQuery.UserId, out var parsedUserId)
                && parsedUserId != Guid.Empty:
                return (parsedUserId, threadDto.ForumId);
        }

        return (Guid.Empty, Guid.Empty);
    }
}
