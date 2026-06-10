using AutoMapper;
using ForumService.ForumService.Application.DTOs;
using ForumService.ForumService.Application.Enums;
using ForumService.ForumService.Application.Interfaces.UnitOfWork;
using ForumService.ForumService.Domain.Entities;
using MediatR;

namespace ForumService.ForumService.Application.Features.Forums.Commands.CreateForum;

public sealed class CreateForumCommandHandler : IRequestHandler<CreateForumCommand, ForumDto?>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public CreateForumCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<ForumDto?> Handle(CreateForumCommand request, CancellationToken cancellationToken)
    {
        var forum = new Forum
        {
            Id = Guid.NewGuid(),
            Name = request.Forum.Name,
            ShortName = request.Forum.ShortName,
            CreatedAt = DateTime.UtcNow,
            Description = request.Forum.Description,
            ForumBanner = request.Forum.ForumBanner,
            ForumImage = request.Forum.ForumImage,
            CreatorId = request.UserId,
        };

        var insertedForum = await _unitOfWork.ForumRepo.InsertForumAsync(forum);
        if (insertedForum == null)
            return null;

        await _unitOfWork.ForumRepo.InsertUserToForumAsync(forum.Id, request.UserId, ForumRole.Admin);
        await _unitOfWork.CommitAsync();

        return _mapper.Map<ForumDto>(insertedForum);
    }
}
