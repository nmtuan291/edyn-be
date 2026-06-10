using ForumService.ForumService.Application.Interfaces.Services;
using ForumService.ForumService.Application.Requests;
using MediatR;

namespace ForumService.ForumService.Application.Features.Forums.Commands.SetMemberRole;

public sealed class SetMemberRoleCommandHandler : IRequestHandler<SetMemberRoleCommand>
{
    private readonly IPermissionService _permissionService;

    public SetMemberRoleCommandHandler(IPermissionService permissionService)
    {
        _permissionService = permissionService;
    }

    public async Task Handle(SetMemberRoleCommand request, CancellationToken cancellationToken)
    {
        await _permissionService.SetUserRoleAsync(
            new ForumMemberRoleUpdate(
                request.ForumId,
                request.TargetUserId,
                request.Role,
                request.ActingUserId));
    }
}
