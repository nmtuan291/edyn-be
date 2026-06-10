using ForumService.ForumService.Application.DTOs;
using ForumService.ForumService.Application.Requests;
using MediatR;

namespace ForumService.ForumService.Application.Features.Comments.Commands.EditComment;

public sealed record EditCommentCommand(
    Guid CommentId,
    Guid UserId,
    EditCommentRequest Request) : IRequest<CommentDto?>;
