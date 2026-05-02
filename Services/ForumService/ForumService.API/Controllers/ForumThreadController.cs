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
        public async Task<ActionResult<List<ForumThreadDto>>> GetForumThreads(Guid forumId, CancellationToken cancellationToken)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var forumThreads = await _forumThreadService.GetThreadsByForumId(
                new ForumThreadListQuery(forumId, userId, 1, 10), cancellationToken);
            return Ok(forumThreads);
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
    }
}