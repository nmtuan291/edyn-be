using AuthService.AuthService.Application.Dtos;
using AuthService.Dtos;
using AuthService.Entities;
using AuthService.Exceptions;
using AuthService.Interfaces.Repositories;
using AuthService.Interfaces.Services;
using AuthService.Services.Security;
using Grpc.Core;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Polly;
using Polly.Retry;
using UserService.Grpc;

namespace AuthService.Services;

public class AccountService : IAccountService
{
    private readonly UserManager<Account> _userManager;
    private readonly ITokenService _tokenService;
    private readonly UserProfileService.UserProfileServiceClient _profileService;
    private readonly RsaKeyProvider  _rsaKeyProvider;
    private readonly IConfiguration _config;
    private readonly ITokenRepository _tokenRepository;
    private readonly AsyncRetryPolicy _retryPolicy;

    private const int CreateProfileRetryCount = 3;
    private const int RefreshTokenLifetimeDays = 14;
    
    public AccountService(UserManager<Account> userManager, ITokenService tokenService, 
        UserProfileService.UserProfileServiceClient profileService,  RsaKeyProvider rsaKeyProvider, IConfiguration config
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
            .WaitAndRetryAsync(CreateProfileRetryCount, retryAttempt => TimeSpan.FromSeconds(retryAttempt));
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
        
        RsaSecurityKey privateKey = _rsaKeyProvider.GetPrivateKey(_config.GetSection("Rsa"), _config["Rsa:Kid"]!);
        string token = _tokenService.GenerateJwtToken(new JwtTokenGenerationRequest
        {
            UserId = user.Id,
            Email = user.Email ?? "",
            Username = user.UserName ?? "",
            PrivateKey = privateKey
        });
        
        RefreshToken newRefreshToken = Entities.RefreshToken.GenerateRefreshToken(user.Id, RefreshTokenLifetimeDays);
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
        if (token == null || !token.IsActive())
            throw new RefreshTokenException("Refresh token is expired or invalid");
        
        if (token.ValidateToken(refreshToken))
        {
            RefreshToken newRefreshToken = Entities.RefreshToken.GenerateRefreshToken(accountId, RefreshTokenLifetimeDays);
            RsaSecurityKey privateKey = _rsaKeyProvider.GetPrivateKey(_config.GetSection("Rsa"), _config["Rsa:Kid"]!);
            string newJwtToken = _tokenService.GenerateJwtToken(new JwtTokenGenerationRequest
            {
                UserId = account.Id,
                Email = account.Email ?? "",
                Username = account.UserName ?? "",
                PrivateKey = privateKey
            });
            
            token.UpdateRefreshToken(newRefreshToken);
            await _tokenRepository.SaveChangesAsync();
            
            return new TokenResponse()
            {
                AccessToken = newJwtToken,
                RefreshToken = newRefreshToken.Token,
            };
        }
        else
        {
            throw new RefreshTokenException("Refresh token validation failed");
        }
    }

    public async Task<IdentityResult> Register(RegisterAccountDto account)
    {
        if (account.Password != account.PasswordVerify)
            return IdentityResult.Failed(new IdentityError { Code = "PasswordMismatch", Description = "Passwords do not match" });

        var user = new Account
        {
            Email = account.Email,
            UserName = account.Username,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
        };
        
        var userCreated = await _userManager.CreateAsync(user, account.Password);
        if (!userCreated.Succeeded)
            return userCreated;

        try
        {
            CreateProfileRequest request = new()
            {
                Id = user.Id,
                Username = user.UserName,
                Email = user.Email,
                Gender = account.Gender,
            };

            await _retryPolicy.ExecuteAsync(async () =>
            {
                var gRpcResult = await _profileService.CreateProfileAsync(request);

                if (!gRpcResult.Success)
                    throw new CreateProfileException(gRpcResult.Message);
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

    public async Task Logout(string accountId, string refreshToken)
    {
        await _tokenRepository.RevokeRefreshTokenAsync(accountId, refreshToken);
    }

    public async Task<IdentityResult> ChangePassword(string accountId, ChangePasswordDto request)
    {
        if (request.NewPassword != request.NewPasswordVerify)
            return IdentityResult.Failed(new IdentityError { Code = "PasswordMismatch", Description = "New passwords do not match" });

        var user = await _userManager.FindByIdAsync(accountId);
        if (user == null)
            return IdentityResult.Failed(new IdentityError { Code = "UserNotFound", Description = "User not found" });

        var result = await _userManager.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword);
        if (result.Succeeded)
        {
            // Revoke all existing refresh tokens to force re-login on other devices
            await _tokenRepository.RevokeAllRefreshTokensAsync(accountId);
        }

        return result;
    }

    public async Task RevokeAllSessions(string accountId)
    {
        await _tokenRepository.RevokeAllRefreshTokensAsync(accountId);
    }
}