using AutoMapper;
using ForumService.ForumService.Application.DTOs;
using ForumService.ForumService.Application.Interfaces.UnitOfWork;
using MediatR;

namespace ForumService.ForumService.Application.Features.Threads.Queries.SearchThreads;

public sealed class SearchThreadsQueryHandler : IRequestHandler<SearchThreadsQuery, PagedResult<ForumThreadDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public SearchThreadsQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
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

        var threads = await _unitOfWork.ThreadRepo.SearchThreadsAsync(
            request.Query,
            request.Page,
            request.PageSize,
            cancellationToken);

        var totalCount = await _unitOfWork.ThreadRepo.SearchThreadsCountAsync(
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
