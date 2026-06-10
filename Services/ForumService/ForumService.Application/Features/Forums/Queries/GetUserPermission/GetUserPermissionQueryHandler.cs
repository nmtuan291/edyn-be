using ForumService.ForumService.Application.DTOs;
using ForumService.ForumService.Application.Enums;
using ForumService.ForumService.Application.Exceptions;
using ForumService.ForumService.Application.Interfaces.Services;
using ForumService.ForumService.Application.Interfaces.UnitOfWork;
using MediatR;

namespace ForumService.ForumService.Application.Features.Forums.Queries.GetUserPermission;

public sealed class GetUserPermissionQueryHandler : IRequestHandler<GetUserPermissionQuery, MemberPermissionDto?>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPermissionService _permissionService;

    public GetUserPermissionQueryHandler(IUnitOfWork unitOfWork, IPermissionService permissionService)
    {
        _unitOfWork = unitOfWork;
        _permissionService = permissionService;
    }

    public async Task<MemberPermissionDto?> Handle(GetUserPermissionQuery request, CancellationToken cancellationToken)
    {
        var forum = await _unitOfWork.ForumRepo.GetForumByIdAsync(request.ForumId, cancellationToken);
        if (forum == null)
            throw new ForumNotFoundException(request.ForumId);

        var forumUser = await _unitOfWork.ForumRepo.GetForumUserAsync(request.ForumId, request.UserId, cancellationToken);
        if (forumUser == null)
            return null;

        var effective = await _permissionService.GetEffectivePermissionsAsync(
            request.ForumId,
            request.UserId,
            cancellationToken);

        return new MemberPermissionDto
        {
            UserId = request.UserId,
            ForumId = request.ForumId,
            Role = forumUser.Role,
            EffectivePermissions = effective,
            PermissionOverrides = forumUser.PermissionOverrides.HasValue
                ? (ForumPermissionType)forumUser.PermissionOverrides.Value
                : null,
        };
    }
}
