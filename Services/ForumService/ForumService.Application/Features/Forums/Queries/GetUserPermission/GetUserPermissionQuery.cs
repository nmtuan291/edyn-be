using ForumService.ForumService.Application.DTOs;
using MediatR;

namespace ForumService.ForumService.Application.Features.Forums.Queries.GetUserPermission;

public sealed record GetUserPermissionQuery(
    Guid ForumId,
    Guid UserId) : IRequest<MemberPermissionDto?>;
