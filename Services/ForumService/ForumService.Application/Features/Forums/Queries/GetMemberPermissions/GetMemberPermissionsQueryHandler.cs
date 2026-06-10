using ForumService.ForumService.Application.Enums;
using ForumService.ForumService.Application.Features.Forums.Queries.GetUserPermission;
using ForumService.ForumService.Application.Interfaces.Services;
using MediatR;

namespace ForumService.ForumService.Application.Features.Forums.Queries.GetMemberPermissions;

public sealed class GetMemberPermissionsQueryHandler
    : IRequestHandler<GetMemberPermissionsQuery, GetMemberPermissionsResult>
{
    private readonly IPermissionService _permissionService;
    private readonly IMediator _mediator;

    public GetMemberPermissionsQueryHandler(IPermissionService permissionService, IMediator mediator)
    {
        _permissionService = permissionService;
        _mediator = mediator;
    }

    public async Task<GetMemberPermissionsResult> Handle(
        GetMemberPermissionsQuery request,
        CancellationToken cancellationToken)
    {
        if (request.ActingUserId != request.TargetUserId)
        {
            var hasPermission = await _permissionService.HasPermissionAsync(
                request.ForumId,
                request.ActingUserId,
                ForumPermissionType.ManageRoles,
                cancellationToken);

            if (!hasPermission)
                return new GetMemberPermissionsResult(null, Forbidden: true);
        }

        var permissions = await _mediator.Send(
            new GetUserPermissionQuery(request.ForumId, request.TargetUserId),
            cancellationToken);

        return new GetMemberPermissionsResult(permissions);
    }
}
