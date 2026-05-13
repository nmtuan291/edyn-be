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

/// <summary>
/// Abstract base class providing shared authentication infrastructure
/// for both traditional (password) and OAuth-based login flows.
/// </summary>
public abstract class BaseAuthService
{
    protected readonly UserManager<Account> UserManager;
    protected readonly ITokenService TokenService;
    protected readonly UserProfileService.UserProfileServiceClient ProfileService;
    protected readonly RsaKeyProvider RsaKeyProvider;
    protected readonly IConfiguration Config;
    protected readonly ITokenRepository TokenRepository;
    protected readonly AsyncRetryPolicy RetryPolicy;

    protected const int CreateProfileRetryCount = 3;
    protected const int RefreshTokenLifetimeDays = 14;

    protected BaseAuthService(
        UserManager<Account> userManager,
        ITokenService tokenService,
        UserProfileService.UserProfileServiceClient profileService,
        RsaKeyProvider rsaKeyProvider,
        IConfiguration config,
        ITokenRepository tokenRepository)
    {
        UserManager = userManager;
        TokenService = tokenService;
        ProfileService = profileService;
        RsaKeyProvider = rsaKeyProvider;
        Config = config;
        TokenRepository = tokenRepository;
        RetryPolicy = Policy
            .Handle<Exception>()
            .WaitAndRetryAsync(CreateProfileRetryCount, retryAttempt => TimeSpan.FromSeconds(retryAttempt));
    }

    public async Task<TokenResponse> RefreshToken(string accountId, string refreshToken)
    {
        var account = await UserManager.Users
            .Include(u => u.RefreshTokens)
            .SingleOrDefaultAsync(u => u.Id == accountId);
        
        if (account == null)
            throw new RefreshTokenException("Account not found");
        
        var token = account.RefreshTokens.FirstOrDefault(t => t.Token == refreshToken);
        if (token == null || !token.IsActive())
            throw new RefreshTokenException("Refresh token is expired or invalid");
        
        if (!token.ValidateToken(refreshToken))
            throw new RefreshTokenException("Refresh token validation failed");

        RefreshToken newRefreshToken = Entities.RefreshToken.GenerateRefreshToken(accountId, RefreshTokenLifetimeDays);
        RsaSecurityKey privateKey = RsaKeyProvider.GetPrivateKey(Config.GetSection("Rsa"), Config["Rsa:Kid"]!);
        string newJwtToken = TokenService.GenerateJwtToken(new JwtTokenGenerationRequest
        {
            UserId = account.Id,
            Email = account.Email ?? "",
            Username = account.UserName ?? "",
            PrivateKey = privateKey
        });
        
        token.UpdateRefreshToken(newRefreshToken);
        await TokenRepository.SaveChangesAsync();
        
        return new TokenResponse()
        {
            AccessToken = newJwtToken,
            RefreshToken = newRefreshToken.Token,
        };
    }

    public async Task Logout(string accountId, string refreshToken)
    {
        await TokenRepository.RevokeRefreshTokenAsync(accountId, refreshToken);
    }

    public async Task RevokeAllSessions(string accountId)
    {
        await TokenRepository.RevokeAllRefreshTokensAsync(accountId);
    }

    protected async Task<LoginResponseDto> IssueTokens(Account account)
    {
        RsaSecurityKey privateKey = RsaKeyProvider.GetPrivateKey(Config.GetSection("Rsa"), Config["Rsa:Kid"]!);
        string token = TokenService.GenerateJwtToken(new JwtTokenGenerationRequest
        {
            UserId = account.Id,
            Email = account.Email ?? "",
            Username = account.UserName ?? "",
            PrivateKey = privateKey
        });

        RefreshToken newRefreshToken = Entities.RefreshToken.GenerateRefreshToken(account.Id, RefreshTokenLifetimeDays);
        await TokenRepository.SaveRefreshToken(newRefreshToken);
        await TokenRepository.SaveChangesAsync();

        return new LoginResponseDto
        {
            Id = account.Id,
            Email = account.Email ?? "",
            AccessToken = token,
            RefreshToken = newRefreshToken.Token,
            UserName = account.UserName ?? "",
        };
    }

    protected async Task CreateUserProfile(Account account, int gender = 0)
    {
        var request = new CreateProfileRequest
        {
            Id = account.Id,
            Username = account.UserName,
            Email = account.Email,
            Gender = gender,
        };

        await RetryPolicy.ExecuteAsync(async () =>
        {
            var gRpcResult = await ProfileService.CreateProfileAsync(request);
            if (!gRpcResult.Success)
                throw new CreateProfileException(gRpcResult.Message);
        });
    }
}
