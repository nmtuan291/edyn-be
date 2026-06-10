using ForumService.ForumService.Application.DTOs;
using MediatR;

namespace ForumService.ForumService.Application.Features.Forums.Queries.GetForumByName;

public sealed record GetForumByNameQuery(
    Guid UserId,
    string ForumName) : IRequest<ForumDto?>;
