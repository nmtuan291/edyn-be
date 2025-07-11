using AuthService.AuthService.Application.Dtos;
using AuthService.AuthService.Application.Interfaces.Services;
using AuthService.AuthService.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using UserService.Grpc;

namespace AuthService.AuthService.Application;

public class AccountService : IAccountService
{
    private readonly UserManager<Account> _userManager;
    private readonly ITokenService _tokenService;
    private readonly ProfileService.ProfileServiceClient _profileService;
    
    public AccountService(UserManager<Account> userManager, ITokenService tokenService, ProfileService.ProfileServiceClient profileService)
    {
        _userManager = userManager;
        _tokenService = tokenService;
        _profileService = profileService;
    }
    
    public async Task<LoginResponseDto?> Login(LoginAccountDto account)
    {
        var user = account.IsEmail ? 
            await _userManager.FindByEmailAsync(account.Username) :
            await _userManager.FindByNameAsync(account.Username);

        if (user == null)
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
            UserName = account.Username,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
        };
        
        var result = await _userManager.CreateAsync(user, account.Password);

        CreateProfileRequest request = new()
        {
            Id = user.Id,
            Username = user.UserName,
            Email = user.Email,
            Gender = account.Gender,
        };
        var gRpcResult = _profileService.CreateProfile(request);
        
        return result;
    }

    public async Task<bool> VerifyEmail(string email)
    {
        var user = await _userManager.FindByEmailAsync(email);
        return user != null;
    }

    public async Task<bool> VerifyUsername(string username)
    {
        var user = await _userManager.FindByNameAsync(username);
        return user != null;
    } 
};