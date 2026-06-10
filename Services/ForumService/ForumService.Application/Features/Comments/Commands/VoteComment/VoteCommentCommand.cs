using ForumService.ForumService.Application.DTOs;
using MediatR;

namespace ForumService.ForumService.Application.Features.Comments.Commands.VoteComment;

public sealed record VoteCommentCommand(
    Guid CommentId,
    Guid UserId,
    bool IsDownVote) : IRequest<CommentDto?>;
