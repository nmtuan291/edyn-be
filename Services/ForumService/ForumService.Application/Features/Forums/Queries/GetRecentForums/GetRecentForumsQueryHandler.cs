using AutoMapper;
using ForumService.ForumService.Application.DTOs;
using ForumService.ForumService.Application.Interfaces.UnitOfWork;
using MediatR;

namespace ForumService.ForumService.Application.Features.Forums.Queries.GetRecentForums;

public sealed class GetRecentForumsQueryHandler : IRequestHandler<GetRecentForumsQuery, List<ForumDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetRecentForumsQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<List<ForumDto>> Handle(GetRecentForumsQuery request, CancellationToken cancellationToken)
    {
        var forums = await _unitOfWork.ForumRepo.GetRecentVisitForumsAsync(request.UserId, cancellationToken);
        return _mapper.Map<List<ForumDto>>(forums);
    }
}
