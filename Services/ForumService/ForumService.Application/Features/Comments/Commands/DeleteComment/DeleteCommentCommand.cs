using MediatR;

namespace ForumService.ForumService.Application.Features.Comments.Commands.DeleteComment;

public sealed record DeleteCommentCommand(
    Guid CommentId,
    Guid UserId) : IRequest;
