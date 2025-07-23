using System.Security.Claims;
using ForumService.ForumService.Application.DTOs;
using Microsoft.AspNetCore.Mvc;
using ForumService.ForumService.Application.Interfaces.Services;
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
        public async Task<ActionResult> Create(ForumThreadDto forumThread)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("Invalid user id");
            }
            
            await _forumThreadService.CreateForumThread(forumThread, Guid.Parse(userId));
            return Ok();
        }
    }
}