using ForumService.ForumService.Application.DTOs;
using ForumService.ForumService.Application.Requests;
using MediatR;

namespace ForumService.ForumService.Application.Features.Forums.Commands.CreateForumTag;

public sealed record CreateForumTagCommand(
    Guid ForumId,
    CreateForumTagRequest Request,
    Guid ActingUserId) : IRequest<ForumTagDto>;
