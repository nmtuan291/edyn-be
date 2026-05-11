using System.Security.Claims;
using ForumService.ForumService.Application.DTOs;
using ForumService.ForumService.Application.Enums;
using ForumService.ForumService.Application.Interfaces.Services;
using ForumService.ForumService.Application.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ForumService.ForumService.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ForumThreadController : ControllerBase
    {
        private readonly IForumThreadService _forumThreadService;
        private readonly IPermissionService _permissionService;

        public ForumThreadController(IForumThreadService forumThreadService, IPermissionService permissionService)
        {
            _forumThreadService = forumThreadService;
            _permissionService = permissionService;
        }

        [Authorize]
        [HttpPost]
        public async Task<ActionResult> CreateThread(ForumThreadDto forumThread)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized("Invalid user id");

            var userGuid = Guid.Parse(userId);
            if (!await _permissionService.HasPermissionAsync(forumThread.ForumId, userGuid, ForumPermissionType.CreateThread))
                return Forbid();

            var created = await _forumThreadService.CreateForumThread(forumThread, userGuid);
            return Ok(created);
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
            var result = await _forumThreadService.GetThreadsByForumIdPaged(
                new ForumThreadListQuery(forumId, userId, page, pageSize, sortBy, sortDate), cancellationToken);
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

            var result = await _forumThreadService.SearchThreads(q, page, pageSize, cancellationToken);
            return Ok(result);
        }

        [HttpGet("thread/{threadId}")]
        public async Task<ActionResult<ForumThreadDto>> GetThreadById(Guid threadId, CancellationToken cancellationToken)
        {
            var thread = await _forumThreadService.GetThreadById(threadId, cancellationToken);
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
            var thread = await _forumThreadService.GetThreadById(comment.ThreadId);
            if (thread == null)
                return NotFound("Thread not found");

            if (!await _permissionService.HasPermissionAsync(thread.ForumId, userGuid, ForumPermissionType.CreateComment))
                return Forbid();

            await _forumThreadService.InsertComment(comment, userGuid, username);
            return Ok();
        }

        [AllowAnonymous]
        [HttpGet("{threadId}/comments")]
        public async Task<ActionResult<List<CommentDto>>> GetComments(Guid threadId, CancellationToken cancellationToken)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var comments = await _forumThreadService.GetCommentsByThreadId(threadId, userId, cancellationToken);
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
            var thread = await _forumThreadService.GetThreadById(threadId);
            if (thread == null)
                return NotFound("Thread not found");

            if (!await _permissionService.HasPermissionAsync(thread.ForumId, userGuid, ForumPermissionType.Vote))
                return Forbid();

            var result = await _forumThreadService.UpdateThreadVote(threadId, userGuid, voteRequest.GetIsDownvote());
            if (result == null)
                return NotFound("Thread not found");
            return Ok(result);
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

            var result = await _forumThreadService.UpdateCommentVote(commentId, userGuid, voteRequest.GetIsDownvote());
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

            var result = await _forumThreadService.EditThread(threadId, Guid.Parse(userId), request);
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

            await _forumThreadService.DeleteThread(threadId, Guid.Parse(userId));
            return Ok();
        }

        [Authorize]
        [HttpPut("comment/{commentId}")]
        public async Task<ActionResult<CommentDto?>> EditComment(Guid commentId, [FromBody] EditCommentRequest request)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var result = await _forumThreadService.EditComment(commentId, Guid.Parse(userId), request);
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

            await _forumThreadService.DeleteComment(commentId, Guid.Parse(userId));
            return Ok();
        }
    }
}