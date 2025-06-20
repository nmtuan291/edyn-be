using Microsoft.AspNetCore.Mvc;
using UserService.UserService.Application.Dtos;
using UserService.UserService.Application.Interfaces.Services;

namespace UserService.UserService.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class UserController : ControllerBase
{
    private readonly ILogger<UserController> _logger;
    private readonly IUserProfileService _userProfileService;

    public UserController(ILogger<UserController> logger, IUserProfileService userProfileService)
    {
        _logger = logger;
        _userProfileService = userProfileService;
    }
    
    [HttpGet("{accountId}")]
    public async Task<ActionResult<UserProfileDto>> GetUserProfile(string accountId)
    {
        var userProfile = await _userProfileService.GetUserById(accountId);
        
        return Ok(userProfile);
    }
}