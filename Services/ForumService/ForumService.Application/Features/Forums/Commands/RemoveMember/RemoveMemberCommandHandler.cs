using ForumService.ForumService.Application.Enums;
using ForumService.ForumService.Application.Interfaces.Services;
using ForumService.ForumService.Application.Interfaces.UnitOfWork;
using MediatR;

namespace ForumService.ForumService.Application.Features.Forums.Commands.RemoveMember;

public sealed class RemoveMemberCommandHandler : IRequestHandler<RemoveMemberCommand>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPermissionService _permissionService;

    public RemoveMemberCommandHandler(IUnitOfWork unitOfWork, IPermissionService permissionService)
    {
        _unitOfWork = unitOfWork;
        _permissionService = permissionService;
    }

    public async Task Handle(RemoveMemberCommand request, CancellationToken cancellationToken)
    {
        var hasPermission = await _permissionService.HasPermissionAsync(
            request.ForumId,
            request.ActingUserId,
            ForumPermissionType.BanMember,
            cancellationToken);

        if (!hasPermission)
            throw new UnauthorizedAccessException("You do not have permission to remove members.");

        var targetUser = await _unitOfWork.ForumRepo.GetForumUserAsync(
            request.ForumId,
            request.TargetUserId,
            cancellationToken);

        if (targetUser == null)
            throw new InvalidOperationException("Target user is not a member of this forum.");

        if (targetUser.Role == ForumRole.Admin)
            throw new InvalidOperationException("Cannot remove the forum admin.");

        var actingUser = await _unitOfWork.ForumRepo.GetForumUserAsync(
            request.ForumId,
            request.ActingUserId,
            cancellationToken);

        if (actingUser!.Role >= targetUser.Role)
            throw new UnauthorizedAccessException("Cannot remove a member with equal or higher role.");

        await _unitOfWork.ForumRepo.RemoveForumUserAsync(request.ForumId, request.TargetUserId);
        await _unitOfWork.CommitAsync();
        await _unitOfWork.ForumRepo.InvalidateCachedPermissionsAsync(request.ForumId, request.TargetUserId);
    }
}
