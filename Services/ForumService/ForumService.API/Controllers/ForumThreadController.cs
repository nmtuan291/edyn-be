using Microsoft.AspNetCore.Mvc;
using ForumService.ForumService.Application.Interfaces.Services;

namespace ForumService.ForumService.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ForumThreadController : ControllerBase
    {
        private readonly IForumThreadService _forumThreadService;
    }
}