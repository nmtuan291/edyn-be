using ForumService.ForumService.Application.Enums;
using ForumService.ForumService.Application.Interfaces.UnitOfWork;
using MediatR;

namespace ForumService.ForumService.Application.Features.Forums.Commands.LeaveForum;

public sealed class LeaveForumCommandHandler : IRequestHandler<LeaveForumCommand>
{
    private readonly IUnitOfWork _unitOfWork;

    public LeaveForumCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(LeaveForumCommand request, CancellationToken cancellationToken)
    {
        var forumUser = await _unitOfWork.ForumRepo.GetForumUserAsync(
            request.ForumId,
            request.UserId,
            cancellationToken);

        if (forumUser == null)
            throw new InvalidOperationException("You are not a member of this forum.");

        if (forumUser.Role == ForumRole.Admin)
            throw new InvalidOperationException("Forum admin cannot leave. Transfer ownership first.");

        await _unitOfWork.ForumRepo.RemoveForumUserAsync(request.ForumId, request.UserId);
        await _unitOfWork.CommitAsync();
        await _unitOfWork.ForumRepo.InvalidateCachedPermissionsAsync(request.ForumId, request.UserId);
    }
}
