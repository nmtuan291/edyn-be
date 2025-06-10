using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AuthService.AuthService.Application.Dtos;
using AuthService.AuthService.Application.Interfaces.Repositories;
using AuthService.AuthService.Application.Interfaces.Services;
using AuthService.AuthService.Infrastructure.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Http.Logging;
using Microsoft.IdentityModel.Tokens;
using Xunit;
using JwtRegisteredClaimNames = Microsoft.IdentityModel.JsonWebTokens.JwtRegisteredClaimNames;

namespace AuthService.AuthService.Application;

public class AccountService : IAccountService
{
    private readonly IConfiguration _config;
    private readonly UserManager<IdentityUser> _userManager;
    private readonly ITokenService _tokenService;
    
    public AccountService(IConfiguration config, UserManager<IdentityUser> userManager, ITokenService tokenService)
    {
        _config = config;      
        _userManager = userManager;
        _tokenService = tokenService;
    }

    
    public async Task<LoginResponseDto?> Login(LoginAccountDto account)
    {
        var user = await _userManager.FindByNameAsync(account.Username);
        if (user == null || string.IsNullOrEmpty(user.Email) || string.IsNullOrEmpty(user.UserName))
            return null;
        
        bool isPasswordValid = await _userManager.CheckPasswordAsync(user, account.Password);
        if (!isPasswordValid)
            return null;

        string token = _tokenService.GenerateJwtToken(user.Id, user.Email);

        return new LoginResponseDto
        {
            Id = user.Id,
            Email = user.Email,
            Token = token,
            UserName = user.UserName,
        };
    }


};