using ForumService.ForumService.Application.Interfaces.Services;
using ForumService.ForumService.Application.Requests;
using MediatR;

namespace ForumService.ForumService.Application.Features.Forums.Commands.SetMemberPermissions;

public sealed class SetMemberPermissionsCommandHandler : IRequestHandler<SetMemberPermissionsCommand>
{
    private readonly IPermissionService _permissionService;

    public SetMemberPermissionsCommandHandler(IPermissionService permissionService)
    {
        _permissionService = permissionService;
    }

    public async Task Handle(SetMemberPermissionsCommand request, CancellationToken cancellationToken)
    {
        await _permissionService.SetPermissionOverridesAsync(
            new ForumMemberPermissionOverridesUpdate(
                request.ForumId,
                request.TargetUserId,
                request.Permissions,
                request.ActingUserId));
    }
}
