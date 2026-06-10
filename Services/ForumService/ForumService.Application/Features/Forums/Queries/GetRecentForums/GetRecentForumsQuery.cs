using ForumService.ForumService.Application.DTOs;
using MediatR;

namespace ForumService.ForumService.Application.Features.Forums.Queries.GetRecentForums;

public sealed record GetRecentForumsQuery(Guid UserId) : IRequest<List<ForumDto>>;
