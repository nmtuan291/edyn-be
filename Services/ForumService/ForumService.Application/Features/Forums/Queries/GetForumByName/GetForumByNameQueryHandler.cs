using AutoMapper;
using ForumService.ForumService.Application.DTOs;
using ForumService.ForumService.Application.Interfaces.UnitOfWork;
using MediatR;

namespace ForumService.ForumService.Application.Features.Forums.Queries.GetForumByName;

public sealed class GetForumByNameQueryHandler : IRequestHandler<GetForumByNameQuery, ForumDto?>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetForumByNameQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<ForumDto?> Handle(GetForumByNameQuery request, CancellationToken cancellationToken)
    {
        var forum = await _unitOfWork.ForumRepo.GetForumByNameAsync(
            request.UserId,
            request.ForumName,
            cancellationToken);

        return forum == null ? null : _mapper.Map<ForumDto>(forum);
    }
}
