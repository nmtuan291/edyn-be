using ForumService.ForumService.Application;
using ForumService.ForumService.Application.DTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using ForumService.ForumService.Application.Interfaces.Services;

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
        
        [HttpPost]
        public async Task<ActionResult> CreateForum([FromBody] ForumDto forumDto)
        {
            if (forumDto == null)
                return BadRequest("Forum data is required");
            
            var createdForum = await _forumService.AddForum(forumDto);
            return CreatedAtAction(nameof(GetForum), new { forumId = createdForum.Id }, createdForum);
        }
    }
}
