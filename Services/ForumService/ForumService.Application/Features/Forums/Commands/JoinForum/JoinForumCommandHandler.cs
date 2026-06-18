using ForumService.ForumService.Application.Enums;
using ForumService.ForumService.Application.Exceptions;
using ForumService.ForumService.Application.Interfaces.UnitOfWork;
using MediatR;

namespace ForumService.ForumService.Application.Features.Forums.Commands.JoinForum;

public sealed class JoinForumCommandHandler : IRequestHandler<JoinForumCommand>
{
    private readonly IUnitOfWork _unitOfWork;

    public JoinForumCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(JoinForumCommand request, CancellationToken cancellationToken)
    {
        var forum = await _unitOfWork.ForumRepo.GetForumByIdAsync(request.ForumId, cancellationToken);
        if (forum == null)
            throw new ForumNotFoundException(request.ForumId);

        await _unitOfWork.ForumRepo.InsertUserToForumAsync(request.ForumId, request.UserId, ForumRole.Member);
        await _unitOfWork.CommitAsync();
    }
}
