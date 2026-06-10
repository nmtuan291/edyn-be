using AutoMapper;
using ForumService.ForumService.Application.DTOs;
using ForumService.ForumService.Application.Interfaces.UnitOfWork;
using MediatR;

namespace ForumService.ForumService.Application.Features.Forums.Queries.GetForums;

public sealed class GetForumsQueryHandler : IRequestHandler<GetForumsQuery, List<ForumDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetForumsQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<List<ForumDto>> Handle(GetForumsQuery request, CancellationToken cancellationToken)
    {
        var forums = await _unitOfWork.ForumRepo.GetForumsAsync(cancellationToken);
        return _mapper.Map<List<ForumDto>>(forums);
    }
}
