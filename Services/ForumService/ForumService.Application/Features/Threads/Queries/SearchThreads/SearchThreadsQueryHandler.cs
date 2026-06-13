using AutoMapper;
using ForumService.ForumService.Application.DTOs;
using ForumService.ForumService.Application.Interfaces.Repositories;
using MediatR;

namespace ForumService.ForumService.Application.Features.Threads.Queries.SearchThreads;

public sealed class SearchThreadsQueryHandler : IRequestHandler<SearchThreadsQuery, PagedResult<ForumThreadDto>>
{
    private readonly IThreadQueryRepository _threadRepository;
    private readonly IMapper _mapper;

    public SearchThreadsQueryHandler(IThreadQueryRepository threadRepository, IMapper mapper)
    {
        _threadRepository = threadRepository;
        _mapper = mapper;
    }

    public async Task<PagedResult<ForumThreadDto>> Handle(
        SearchThreadsQuery request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Query))
        {
            return new PagedResult<ForumThreadDto>
            {
                Items = [],
                Page = 1,
                PageSize = request.PageSize,
                TotalCount = 0,
            };
        }

        var threads = await _threadRepository.SearchThreadsAsync(
            request.Query,
            request.Page,
            request.PageSize,
            cancellationToken);

        var totalCount = await _threadRepository.SearchThreadsCountAsync(
            request.Query,
            cancellationToken);

        return new PagedResult<ForumThreadDto>
        {
            Items = _mapper.Map<List<ForumThreadDto>>(threads),
            Page = request.Page,
            PageSize = request.PageSize,
            TotalCount = totalCount,
        };
    }
}
