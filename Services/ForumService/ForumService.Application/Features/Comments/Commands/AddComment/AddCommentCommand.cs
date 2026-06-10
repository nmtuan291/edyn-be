using ForumService.ForumService.Application.DTOs;
using MediatR;

namespace ForumService.ForumService.Application.Features.Comments.Commands.AddComment;

public sealed record AddCommentCommand(
    CommentDto Comment,
    Guid UserId,
    string Username) : IRequest<AddCommentResult>;

public enum AddCommentResult
{
    Created,
    ThreadNotFound,
    Forbidden
}
