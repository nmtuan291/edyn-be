using AuthService.AuthService.Application.Dtos;
using AuthService.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace AuthService.AuthService.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AccountController : ControllerBase
{
    private readonly IAccountService _accountService;

    public AccountController(IAccountService accountService)
    {
        _accountService = accountService;
    }

    [HttpPost("login")]
    public async Task<ActionResult<LoginResponseDto>> Login(LoginAccountDto credentials)
    {
        var response = await _accountService.Login(credentials);
        if (response == null)
            return Unauthorized();
        
        return Ok(response);
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterAccountDto credentials)
    {
        var result = await _accountService.Register(credentials);
        if (result.Succeeded == false)
            return BadRequest(result.Errors);
        
        return Ok();
    }

    [HttpGet("verify-email")]
    public async Task<ActionResult<bool>> VerifyEmail(string email)
    {
        bool exist = await _accountService.VerifyEmail(email);
        if (!exist)
            return Ok(true);    
        
        return BadRequest("Email is already in use");
    }

    [HttpGet("verify-user")]
    public async Task<ActionResult<bool>> VerifyUsername(string username)
    {
        bool exist = await _accountService.VerifyUsername(username);
        if (!exist) 
            return Ok(true);
        return BadRequest("Username is already in use");
    }
}