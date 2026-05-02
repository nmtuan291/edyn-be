using System.Security.Claims;
using ForumService.ForumService.Application.DTOs;
using ForumService.ForumService.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ForumService.ForumService.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FeedController : ControllerBase
{
    private readonly IHomeFeedService _homeFeedService;

    public FeedController(IHomeFeedService homeFeedService)
    {
        _homeFeedService = homeFeedService;
    }

    [AllowAnonymous]
    [HttpGet]
    public async Task<ActionResult<List<ForumThreadDto>>> GetHomeFeed(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        if (page < 1) page = 1;
        if (pageSize is < 1 or > 50) pageSize = 20;

        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var feed = await _homeFeedService.GetHomeFeed(userId, page, pageSize, cancellationToken);
        return Ok(feed);
    }
}
