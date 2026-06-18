using ForumService.ForumService.Application.DTOs;
using MediatR;

namespace ForumService.ForumService.Application.Features.Forums.Queries.GetMemberPermissions;

public sealed record GetMemberPermissionsQuery(
    Guid ForumId,
    Guid TargetUserId,
    Guid ActingUserId) : IRequest<GetMemberPermissionsResult>;

public sealed record GetMemberPermissionsResult(
    MemberPermissionDto? Permissions,
    bool Forbidden = false);
