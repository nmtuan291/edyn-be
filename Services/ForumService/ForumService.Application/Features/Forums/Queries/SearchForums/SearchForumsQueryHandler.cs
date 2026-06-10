using AutoMapper;
using ForumService.ForumService.Application.DTOs;
using ForumService.ForumService.Application.Interfaces.UnitOfWork;
using MediatR;

namespace ForumService.ForumService.Application.Features.Forums.Queries.SearchForums;

public sealed class SearchForumsQueryHandler : IRequestHandler<SearchForumsQuery, List<ForumDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public SearchForumsQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<List<ForumDto>> Handle(SearchForumsQuery request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Query))
            return [];

        var forums = await _unitOfWork.ForumRepo.SearchForumsAsync(request.Query, cancellationToken);
        return _mapper.Map<List<ForumDto>>(forums);
    }
}
