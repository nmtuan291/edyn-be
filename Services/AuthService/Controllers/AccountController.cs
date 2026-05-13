using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using AuthService.AuthService.Application.Dtos;
using AuthService.Interfaces.Services;
using AuthService.Services.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace AuthService.AuthService.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AccountController : ControllerBase
{
    private readonly IAccountService _accountService;
    private readonly IOAuthService _oauthService;
    private readonly RsaKeyProvider _rsaKeyProvider;
    private readonly IConfiguration _config;

    public AccountController(IAccountService accountService, IOAuthService oauthService, RsaKeyProvider rsaKeyProvider, IConfiguration config)
    {
        _accountService = accountService;
        _oauthService = oauthService;
        _rsaKeyProvider = rsaKeyProvider;
        _config = config;
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

    [HttpPost("oauth-login")]
    public async Task<ActionResult<LoginResponseDto>> OAuthLogin([FromBody] OAuthLoginDto dto)
    {
        try
        {
            var response = await _oauthService.OAuthLogin(dto);
            return Ok(response);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
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

    [HttpPost("refresh-token")]
    public async Task<ActionResult<TokenResponse>> RefreshToken([FromBody] RefreshTokenRequest request)
    {
        var validationParameters = new TokenValidationParameters()
        {
            ValidateIssuer = true,
            ValidIssuer = _config["Jwt:Issuer"],
            ValidateAudience = false,
            IssuerSigningKey = _rsaKeyProvider.GetPublicKey(_config.GetSection("Rsa"), _config["Rsa:Kid"]!),
            ValidateLifetime = false
        };
        
        string expiredToken = request.ExpiredToken;
        if (expiredToken.StartsWith("Bearer "))
            expiredToken = expiredToken.Substring("Bearer ".Length);
        
        var handler = new JwtSecurityTokenHandler();
        var principal = handler.ValidateToken(expiredToken, validationParameters, out _);
        if (principal == null)
            return Unauthorized();
        
        var userId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? null;
        if (userId == null)
            return Unauthorized();
        
        return await _accountService.RefreshToken(userId, request.RefreshToken);
    }

    [Authorize]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout([FromBody] LogoutRequest request)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        await _accountService.Logout(userId, request.RefreshToken);
        return Ok();
    }

    [Authorize]
    [HttpPost("change-password")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto request)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        var result = await _accountService.ChangePassword(userId, request);
        if (!result.Succeeded)
            return BadRequest(result.Errors);

        return Ok();
    }

    [Authorize]
    [HttpPost("revoke-all-sessions")]
    public async Task<IActionResult> RevokeAllSessions()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        await _accountService.RevokeAllSessions(userId);
        return Ok();
    }
}