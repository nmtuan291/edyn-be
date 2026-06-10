using AutoMapper;
using ForumService.ForumService.Application.DTOs;
using ForumService.ForumService.Application.Enums;
using ForumService.ForumService.Application.Interfaces.UnitOfWork;
using ForumService.ForumService.Application.Requests;
using MediatR;

namespace ForumService.ForumService.Application.Features.Threads.Queries.GetForumThreads;

public sealed class GetForumThreadsQueryHandler : IRequestHandler<GetForumThreadsQuery, PagedResult<ForumThreadDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetForumThreadsQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<PagedResult<ForumThreadDto>> Handle(
        GetForumThreadsQuery request,
        CancellationToken cancellationToken)
    {
        var page = new ForumThreadPageQuery(
            request.ForumId,
            request.SortBy,
            request.SortDate,
            request.PageNumber,
            request.PageSize);

        var threads = await _unitOfWork.ThreadRepo.GetThreadsByForumIdAsync(page, cancellationToken);
        var votedThreads = string.IsNullOrEmpty(request.UserId)
            ? new Dictionary<Guid, bool>()
            : await _unitOfWork.VoteRepo.GetVotedThreadsAsync(Guid.Parse(request.UserId), request.ForumId);

        var items = _mapper.Map<List<ForumThreadDto>>(threads)
            .Select(dto =>
            {
                if (!string.IsNullOrEmpty(request.UserId))
                {
                    dto.Vote = votedThreads.TryGetValue(dto.Id!.Value, out var isDownVote)
                        ? isDownVote ? VoteStatus.DownVote : VoteStatus.UpVote
                        : VoteStatus.NoVote;
                }
                else
                {
                    dto.Vote = VoteStatus.NoVote;
                }

                return dto;
            })
            .ToList();

        var totalCount = await _unitOfWork.ThreadRepo.GetThreadCountByForumIdAsync(
            request.ForumId,
            cancellationToken);

        return new PagedResult<ForumThreadDto>
        {
            Items = items,
            Page = request.PageNumber,
            PageSize = request.PageSize,
            TotalCount = totalCount,
        };
    }
}
