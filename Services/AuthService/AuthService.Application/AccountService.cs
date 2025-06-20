using AuthService.AuthService.Application.Dtos;
using AuthService.AuthService.Application.Interfaces.Services;
using AuthService.AuthService.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace AuthService.AuthService.Application;

public class AccountService : IAccountService
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly ITokenService _tokenService;
    
    public AccountService(UserManager<IdentityUser> userManager, ITokenService tokenService)
    {
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

    public async Task<AccountDto?> GetAccount(string accountId)
    {
        var account = await _userManager.FindByIdAsync(accountId);

        if (account == null)
        {
            return null;
        }

        return new AccountDto
        {
            Id = account.Id,
            Email = account.Email,
            UserName = account.UserName,
            IsActive = true
        };
    }

    public async Task<IdentityResult> Register(RegisterAccountDto account)
    {
        var user = new Account
        {
            Email = account.Email,
            UserName = account.UserName,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
        };
        
        return await _userManager.CreateAsync(user, account.Password);
    } 
};