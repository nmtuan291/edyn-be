using System.Security.Claims;
using ForumService.ForumService.Application.DTOs;
using Microsoft.AspNetCore.Mvc;
using ForumService.ForumService.Application.Interfaces.Services;
using ForumService.ForumService.Application.Requests;
using Microsoft.AspNetCore.Authorization;

namespace ForumService.ForumService.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ForumThreadController : ControllerBase
    {
        private readonly IForumThreadService _forumThreadService;

        public ForumThreadController(IForumThreadService forumThreadService)
        {
            _forumThreadService = forumThreadService;
        }

        [Authorize]
        [HttpPost]
        public async Task<ActionResult> CreateThread(ForumThreadDto forumThread)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("Invalid user id");
            }
            
            await _forumThreadService.CreateForumThread(forumThread, Guid.Parse(userId));
            return Ok();
        }
        
        [AllowAnonymous]
        [HttpGet("{forumId}")]
        public async Task<ActionResult<List<ForumThreadDto>>> GetForumThreads(Guid forumId) 
        {
            var userId =  User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var forumThreads = await _forumThreadService.GetThreadsByForumId(forumId, userId, 1, 10);
            return Ok(forumThreads);
        }

        [HttpGet("thread/{threadId}")]
        public async Task<ActionResult<ForumThreadDto>> GetThreadById(Guid threadId)
        {
            var thread = await _forumThreadService.GetThreadById(threadId);
            if (thread == null)
                return NotFound("Thread not found");
            return Ok(thread);
        }
        
        [Authorize]
        [HttpPost("comment/create")]
        public async Task<ActionResult> AddComment(CommentDto comment)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var username =  User.FindFirst("name")?.Value;
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(username))
                return Unauthorized("Invalid user id");
            
            await _forumThreadService.InsertComment(comment, Guid.Parse(userId), username);
            return Ok();
        }

        [HttpGet("{threadId}/comments")]
        public async Task<ActionResult<List<CommentDto>>> GetComments(Guid threadId)
        {
            var commments = await _forumThreadService.GetCommentsByThreadId(threadId);
            return Ok(commments);
        }
        
        [Authorize]
        [HttpPost("thread/vote")]
        public async Task<ActionResult<ForumThreadDto?>> Vote(VoteRequest voteRequest)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized("Invalid user id");
            
            return await _forumThreadService.UpdateThreadVote(voteRequest.ThreadId, 
                Guid.Parse(userId), voteRequest.IsDownvote);
        }
    }
}