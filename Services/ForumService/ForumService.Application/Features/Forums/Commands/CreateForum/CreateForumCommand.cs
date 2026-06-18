using ForumService.ForumService.Application.DTOs;
using MediatR;

namespace ForumService.ForumService.Application.Features.Forums.Commands.CreateForum;

public sealed record CreateForumCommand(
    ForumDto Forum,
    Guid UserId) : IRequest<ForumDto?>;
