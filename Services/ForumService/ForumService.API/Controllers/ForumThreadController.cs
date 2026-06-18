using System.Security.Claims;
using ForumService.ForumService.Application.DTOs;
using ForumService.ForumService.Application.Enums;
using ForumService.ForumService.Application.Features.Comments.Commands.AddComment;
using ForumService.ForumService.Application.Features.Comments.Commands.DeleteComment;
using ForumService.ForumService.Application.Features.Comments.Commands.EditComment;
using ForumService.ForumService.Application.Features.Comments.Commands.VoteComment;
using ForumService.ForumService.Application.Features.Threads.Commands.CreateThread;
using ForumService.ForumService.Application.Features.Threads.Commands.DeleteThread;
using ForumService.ForumService.Application.Features.Threads.Commands.EditThread;
using ForumService.ForumService.Application.Features.Threads.Commands.PinThread;
using ForumService.ForumService.Application.Features.Threads.Commands.VotePoll;
using ForumService.ForumService.Application.Features.Threads.Commands.VoteThread;
using ForumService.ForumService.Application.Features.Threads.Queries.GetComments;
using ForumService.ForumService.Application.Features.Threads.Queries.GetForumThreads;
using ForumService.ForumService.Application.Features.Threads.Queries.GetThreadById;
using ForumService.ForumService.Application.Features.Threads.Queries.SearchThreads;
using ForumService.ForumService.Application.Requests;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ForumService.ForumService.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ForumThreadController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ForumThreadController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [Authorize]
        [HttpPost]
        public async Task<ActionResult> CreateThread(ForumThreadDto forumThread)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized("Invalid user id");

            var userGuid = Guid.Parse(userId);
            var result = await _mediator.Send(new CreateThreadCommand(forumThread, userGuid));
            if (result.Forbidden)
                return Forbid();

            return Ok(result.Thread);
        }

        [AllowAnonymous]
        [HttpGet("{forumId}")]
        public async Task<ActionResult<PagedResult<ForumThreadDto>>> GetForumThreads(
            Guid forumId,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] SortBy sortBy = SortBy.Hot,
            [FromQuery] SortDate sortDate = SortDate.All,
            CancellationToken cancellationToken = default)
        {
            if (page < 1) page = 1;
            if (pageSize is < 1 or > 50) pageSize = 10;

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var result = await _mediator.Send(
                new GetForumThreadsQuery(forumId, userId, page, pageSize, sortBy, sortDate),
                cancellationToken);
            return Ok(result);
        }

        [AllowAnonymous]
        [HttpGet("search")]
        public async Task<ActionResult<PagedResult<ForumThreadDto>>> SearchThreads(
            [FromQuery] string q = "",
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(q))
                return Ok(new PagedResult<ForumThreadDto> { Items = [], Page = 1, PageSize = pageSize, TotalCount = 0 });

            if (page < 1) page = 1;
            if (pageSize is < 1 or > 50) pageSize = 10;

            var result = await _mediator.Send(new SearchThreadsQuery(q, page, pageSize), cancellationToken);
            return Ok(result);
        }

        [HttpGet("thread/{threadId}")]
        public async Task<ActionResult<ForumThreadDto>> GetThreadById(Guid threadId, CancellationToken cancellationToken)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var thread = await _mediator.Send(new GetThreadByIdQuery(threadId, userId), cancellationToken);
            if (thread == null)
                return NotFound("Thread not found");
            return Ok(thread);
        }

        [Authorize]
        [HttpPost("comment/create")]
        public async Task<ActionResult> AddComment(CommentDto comment)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var username = User.FindFirst("name")?.Value;
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(username))
                return Unauthorized("Invalid user id");

            var userGuid = Guid.Parse(userId);
            var result = await _mediator.Send(new AddCommentCommand(comment, userGuid, username));
            return result switch
            {
                AddCommentResult.ThreadNotFound => NotFound("Thread not found"),
                AddCommentResult.Forbidden => Forbid(),
                _ => Ok(),
            };
        }

        [AllowAnonymous]
        [HttpGet("{threadId}/comments")]
        public async Task<ActionResult<List<CommentDto>>> GetComments(Guid threadId, CancellationToken cancellationToken)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var comments = await _mediator.Send(new GetCommentsQuery(threadId, userId), cancellationToken);
            return Ok(comments);
        }

        [Authorize]
        [HttpPost("thread/vote")]
        public async Task<ActionResult<ForumThreadDto?>> VoteThread([FromBody] VoteRequest voteRequest)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var threadId = voteRequest.GetThreadId();
            if (threadId == Guid.Empty)
                return BadRequest("Thread id is required (send \"id\" or \"threadId\" from create thread or list threads).");

            var userGuid = Guid.Parse(userId);
            var result = await _mediator.Send(
                new VoteThreadCommand(threadId, userGuid, voteRequest.GetIsDownvote()));
            if (result.Forbidden)
                return Forbid();

            if (result.Thread == null)
                return NotFound("Thread not found");
            return Ok(result.Thread);
        }

        [Authorize]
        [HttpPost("comment/vote")]
        public async Task<ActionResult<CommentDto?>> VoteComment([FromBody] VoteRequest voteRequest)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var commentId = voteRequest.GetCommentId();
            if (commentId == Guid.Empty)
                return BadRequest("Comment id is required (send \"id\" or \"commentId\").");

            var userGuid = Guid.Parse(userId);

            var result = await _mediator.Send(
                new VoteCommentCommand(commentId, userGuid, voteRequest.GetIsDownvote()));
            if (result == null)
                return NotFound("Comment not found");
            return Ok(result);
        }

        [Authorize]
        [HttpPut("thread/{threadId}")]
        public async Task<ActionResult<ForumThreadDto?>> EditThread(Guid threadId, [FromBody] EditThreadRequest request)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var result = await _mediator.Send(new EditThreadCommand(threadId, Guid.Parse(userId), request));
            if (result == null)
                return NotFound("Thread not found");
            return Ok(result);
        }

        [Authorize]
        [HttpPut("thread/{threadId}/pin")]
        public async Task<ActionResult<ForumThreadDto?>> PinThread(Guid threadId, [FromBody] PinThreadRequest request)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var result = await _mediator.Send(new PinThreadCommand(threadId, Guid.Parse(userId), request.IsPinned));
            if (result == null)
                return NotFound("Thread not found");
            return Ok(result);
        }

        [Authorize]
        [HttpDelete("thread/{threadId}")]
        public async Task<ActionResult> DeleteThread(Guid threadId)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            await _mediator.Send(new DeleteThreadCommand(threadId, Guid.Parse(userId)));
            return Ok();
        }

        [Authorize]
        [HttpPut("comment/{commentId}")]
        public async Task<ActionResult<CommentDto?>> EditComment(Guid commentId, [FromBody] EditCommentRequest request)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var result = await _mediator.Send(new EditCommentCommand(commentId, Guid.Parse(userId), request));
            if (result == null)
                return NotFound("Comment not found");
            return Ok(result);
        }

        [Authorize]
        [HttpDelete("comment/{commentId}")]
        public async Task<ActionResult> DeleteComment(Guid commentId)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            await _mediator.Send(new DeleteCommentCommand(commentId, Guid.Parse(userId)));
            return Ok();
        }

        [Authorize]
        [HttpPost("thread/poll-vote")]
        public async Task<ActionResult<ForumThreadDto?>> VotePoll([FromBody] PollVoteRequest request)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            if (request.ThreadId == Guid.Empty || string.IsNullOrWhiteSpace(request.PollContent))
                return BadRequest("ThreadId and PollContent are required.");

            var result = await _mediator.Send(
                new VotePollCommand(Guid.Parse(userId), request.ThreadId, request.PollContent));
            if (result == null)
                return NotFound("Thread or poll item not found.");
            return Ok(result);
        }
    }
}
