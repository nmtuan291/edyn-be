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
        public async Task<ActionResult<IEnumerable<ForumDto>>> GetAllForums()
        {
            var forums = await _forumService.GetForums();
            return Ok(forums);
        }

        [HttpGet("{forumId}")]
        public async Task<ActionResult<ForumDto>> GetForum(Guid forumId)
        {
            if (forumId == Guid.Empty)
                return BadRequest("Forum ID is required");

            var forum = await _forumService.GetForum(forumId);
            return Ok(forum);
        }
        
        [Authorize]
        [HttpPost]
        public async Task<ActionResult> CreateForum([FromBody] ForumDto forumDto)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            
            var createdForum = await _forumService.AddForum(forumDto, userId);
            
            if (createdForum == null)
                return StatusCode(StatusCodes.Status500InternalServerError, "Failed to create forum");
            
            return CreatedAtAction(nameof(GetForum), new { forumId = createdForum.Id }, createdForum);
        }

        [Authorize]
        [HttpPost("join/{forumId}")]
        public async Task<ActionResult> JoinForum(string forumId)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            await _forumService.AddUserToForum(Guid.Parse(forumId), Guid.Parse(userId));
            
            return Ok();
        }
    }
}
