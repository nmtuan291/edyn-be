using ForumService.ForumService.Application.DTOs;
using MediatR;

namespace ForumService.ForumService.Application.Features.Forums.Queries.GetForumTags;

public sealed record GetForumTagsQuery(Guid ForumId) : IRequest<List<ForumTagDto>>;
