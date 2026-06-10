using ForumService.ForumService.Application.DTOs;
using MediatR;

namespace ForumService.ForumService.Application.Features.Forums.Queries.SearchForums;

public sealed record SearchForumsQuery(string Query) : IRequest<List<ForumDto>>;
