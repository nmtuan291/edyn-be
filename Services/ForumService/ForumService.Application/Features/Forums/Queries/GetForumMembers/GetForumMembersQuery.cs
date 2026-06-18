using ForumService.ForumService.Application.DTOs;
using MediatR;

namespace ForumService.ForumService.Application.Features.Forums.Queries.GetForumMembers;

public sealed record GetForumMembersQuery(Guid ForumId) : IRequest<List<ForumMemberDto>>;
