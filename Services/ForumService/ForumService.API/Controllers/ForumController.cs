using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using ForumService.ForumService.Application;
using ForumService.ForumService.Application.DTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using ForumService.ForumService.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;

namespace ForumService.ForumService.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ForumController : ControllerBase
    {
        private readonly IForumService _forumService;

        public ForumController(IForumService forumService)
        {
            _forumService = forumService;
        }

        [HttpGet]
        public async Task<ActionResult<List<ForumDto>>> GetAllForums()
        {
            var forums = await _forumService.GetForums();
            return Ok(forums);
        }


        [Authorize]
        [HttpPost]
        public async Task<ActionResult> CreateForum([FromBody] ForumDto forumDto)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();
            
            var createdForum = await _forumService.AddForum(forumDto, userId);
            
            if (createdForum == null)
                return StatusCode(StatusCodes.Status500InternalServerError, "Failed to create forum");
            
            return CreatedAtAction(nameof(GetForum), new { forumName = createdForum.Name }, createdForum);
        }

        [Authorize]
        [HttpPost("join/{forumId}")]
        public async Task<ActionResult> JoinForum(string forumId)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();
            
            await _forumService.AddUserToForum(Guid.Parse(forumId), Guid.Parse(userId));
            return Ok();
        }

        [HttpGet("{forumName}")]
        public async Task<ActionResult<ForumDto>> GetForum(string forumName)
        {
            if (string.IsNullOrEmpty(forumName))
                return BadRequest("Invalid forum name");
            ForumDto? forum = await _forumService.GetForum(forumName);

            return forum == null ?  NotFound() : forum;
        }

        [Authorize]
        [HttpGet("{forumId}/permissions")]
        public async Task<ActionResult<string?>> GetUserPermissions(string forumId)
        {
            if (!Guid.TryParse(forumId, out var forumGuid))
                return BadRequest("Invalid forum ID");
                
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!Guid.TryParse(userId, out var userGuid) || string.IsNullOrEmpty(userId))
                return Unauthorized("Invalid user ID");

            string? permissions = await _forumService.GetUserPermission(Guid.Parse(forumId), Guid.Parse(userId));
            return permissions;
        }

        [Authorize]
        [HttpGet("/forums/joined")]
        public async Task<ActionResult<List<ForumUserDto>>> GetAllJoinedForums()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized();
            
            var forums = await _forumService.GetJoinedForums(Guid.Parse(userId));
            return forums;
        }
    }
}
