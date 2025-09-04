using System.Data;
using AuthService.AuthService.Application.Dtos;
using AuthService.Entities;
using AuthService.Exceptions;
using AuthService.Interfaces.Repositories;
using AuthService.Interfaces.Services;
using AuthService.Services.Sercurity;
using Grpc.Core;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Polly;
using Polly.Retry;
using UserService.Grpc;

namespace AuthService.Services;

// TODO: Refresh token
public class AccountService : IAccountService
{
    private readonly UserManager<Account> _userManager;
    private readonly ITokenService _tokenService;
    private readonly ProfileService.ProfileServiceClient _profileService;
    private readonly RsaKeyProvider  _rsaKeyProvider;
    private readonly IConfiguration _config;
    private readonly ITokenRepository _tokenRepository;
    private readonly AsyncRetryPolicy _retryPolicy;
    
    public AccountService(UserManager<Account> userManager, ITokenService tokenService, 
        ProfileService.ProfileServiceClient profileService,  RsaKeyProvider rsaKeyProvider, IConfiguration config
        , ITokenRepository tokenRepository)
    {
        _userManager = userManager;
        _tokenService = tokenService;
        _profileService = profileService;
        _rsaKeyProvider = rsaKeyProvider;
        _config = config;
        _tokenRepository = tokenRepository;
        _retryPolicy = Policy
            .Handle<Exception>()
            .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(retryAttempt));
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
        
        RsaSecurityKey privateKey = _rsaKeyProvider.GetPrivateKey(_config["Rsa:PrivateKeyDirectory"], _config["Rsa:Kid"]);
        string token = _tokenService.GenerateJwtToken(user.Id, user.Email, user.UserName, privateKey);
        
        RefreshToken newRefreshToken = Entities.RefreshToken.GenerateRefreshToken(user.Id, 14);
        await _tokenRepository.SaveRefreshToken(newRefreshToken);
        await _tokenRepository.SaveChangesAsync();
        
        return new LoginResponseDto
        {
            Id = user.Id,
            Email = user.Email,
            AccessToken = token,
            RefreshToken = newRefreshToken.Token,
            UserName = user.UserName,
        };
    }

    public async Task<AccountDto?> GetAccount(string accountId)
    {
        var account = await _userManager.FindByIdAsync(accountId);

        if (account == null)
            return null;

        return new AccountDto
        {
            Id = account.Id,
            Email = account.Email ?? "",
            UserName = account.UserName ?? "",
            IsActive = true
        };
    }

    public async Task<TokenResponse> RefreshToken(string accountId, string refreshToken)
    {
        var account = await _userManager.Users
            .Include(u => u.RefreshTokens)
            .SingleOrDefaultAsync(u => u.Id == accountId);
        
        if (account == null)
            throw new RefreshTokenException("Account not found");
        
        var token = account.RefreshTokens.FirstOrDefault(t => t.Token == refreshToken);
        if (token == null)
            throw new RefreshTokenException("Refresh token not found");
        
        RefreshToken newRefreshToken = Entities.RefreshToken.GenerateRefreshToken(accountId, 14);
        RsaSecurityKey privateKey = _rsaKeyProvider.GetPrivateKey(_config["Rsa:PrivateKeyDirectory"], _config["Rsa:Kid"]);
        string newJwtToken = _tokenService.GenerateJwtToken(account.Id, account.Email, account.UserName, privateKey);
        
        if (token.ValidateToken(refreshToken))
            token.UpdateRefreshToken(newRefreshToken);
        await _tokenRepository.SaveChangesAsync();
        
        return new TokenResponse()
        {
            AccessToken = newJwtToken,
            RefreshToken = newRefreshToken.Token,
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
        
        var userCreated = await _userManager.CreateAsync(user, account.Password);
        if (!userCreated.Succeeded)
            return IdentityResult.Failed();

        try
        {
            CreateProfileRequest request = new()
            {
                Id = user.Id,
                Username = user.UserName,
                Email = user.Email,
                Gender = account.Gender,
            };

            await _retryPolicy.ExecuteAsync(() =>
            {
                var gRpcResult = _profileService.CreateProfile(request);

                if (!gRpcResult.Success)
                    throw new CreateProfileException(gRpcResult.Message);
                
                return Task.CompletedTask;
            });
        }
        catch (RpcException ex)
        {
            await _userManager.DeleteAsync(user);
            return IdentityResult.Failed(new IdentityError() { Description = ex.Status.Detail });
        }
        catch (CreateProfileException ex)
        {
            await _userManager.DeleteAsync(user);
            return IdentityResult.Failed(new IdentityError { Description = ex.Message });
        }
        
        return IdentityResult.Success;
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