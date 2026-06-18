using ForumService.ForumService.Application.DTOs;
using MediatR;

namespace ForumService.ForumService.Application.Features.Forums.Queries.GetJoinedForums;

public sealed record GetJoinedForumsQuery(Guid UserId) : IRequest<List<ForumUserDto>>;
