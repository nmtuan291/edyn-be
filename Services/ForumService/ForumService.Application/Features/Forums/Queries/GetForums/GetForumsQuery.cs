using ForumService.ForumService.Application.DTOs;
using MediatR;

namespace ForumService.ForumService.Application.Features.Forums.Queries.GetForums;

public sealed record GetForumsQuery : IRequest<List<ForumDto>>;
