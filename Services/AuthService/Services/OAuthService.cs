using AuthService.AuthService.Application.Dtos;
using AuthService.Data;
using AuthService.Entities;
using AuthService.Exceptions;
using AuthService.Interfaces.Repositories;
using AuthService.Interfaces.Services;
using AuthService.Services.Security;
using Grpc.Core;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using UserService.Grpc;

namespace AuthService.Services;

public class OAuthService : BaseAuthService, IOAuthService
{
    private readonly AuthDbContext _dbContext;
    private readonly IEnumerable<IOAuthValidator> _oauthValidators;

    public OAuthService(
        UserManager<Account> userManager,
        ITokenService tokenService,
        UserProfileService.UserProfileServiceClient profileService,
        RsaKeyProvider rsaKeyProvider,
        IConfiguration config,
        ITokenRepository tokenRepository,
        AuthDbContext dbContext,
        IEnumerable<IOAuthValidator> oauthValidators)
        : base(userManager, tokenService, profileService, rsaKeyProvider, config, tokenRepository)
    {
        _dbContext = dbContext;
        _oauthValidators = oauthValidators;
    }

    public async Task<LoginResponseDto> OAuthLogin(OAuthLoginDto dto)
    {
        var provider = dto.Provider.ToLowerInvariant();

        var validator = _oauthValidators.FirstOrDefault(v => v.Provider == provider)
                        ?? throw new ArgumentException($"Unsupported OAuth2 provider: {dto.Provider}");

        if (!validator.IsConfigured())
            throw new ArgumentException($"OAuth2 provider '{dto.Provider}' is not configured on the server.");

        await validator.ValidateTokenAsync(dto.IdToken);
        var userInfo = await validator.GetUserInfoAsync(dto.IdToken);

        // Try to find an existing OAuth login
        var oauthLogin = await _dbContext.OAuthLogins
            .Include(e => e.Account)
            .FirstOrDefaultAsync(e => e.Provider == provider && e.ProviderUserId == userInfo.ProviderUserId);

        Account? account;

        if (oauthLogin != null)
        {
            account = oauthLogin.Account;
        }
        else
        {
            account = await UserManager.FindByEmailAsync(userInfo.Email);

            if (account == null)
            {
                account = new Account
                {
                    Email = userInfo.Email,
                    UserName = GenerateUsername(userInfo.Name),
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    EmailConfirmed = true // OAuth2 emails are pre-verified
                };

                var createResult = await UserManager.CreateAsync(account);
                if (!createResult.Succeeded)
                {
                    var errors = string.Join(", ", createResult.Errors.Select(e => e.Description));
                    throw new InvalidOperationException($"Failed to create account: {errors}");
                }

                // Create user profile via gRPC
                try
                {
                    await CreateUserProfile(account);
                }
                catch (RpcException ex)
                {
                    await UserManager.DeleteAsync(account);
                    throw new InvalidOperationException($"Failed to create user profile: {ex.Status.Detail}");
                }
                catch (CreateProfileException ex)
                {
                    await UserManager.DeleteAsync(account);
                    throw new InvalidOperationException($"Failed to create user profile: {ex.Message}");
                }
            }

            var newOAuthLogin = new OAuthLogin
            {
                Id = Guid.NewGuid(),
                Provider = provider,
                ProviderUserId = userInfo.ProviderUserId,
                AccountId = account.Id,
                CreatedAt = DateTime.UtcNow
            };

            _dbContext.OAuthLogins.Add(newOAuthLogin);
            await _dbContext.SaveChangesAsync();
        }

        return await IssueTokens(account);
    }

    /// <summary>
    /// Generate a unique username from the user's display name.
    /// Strips non-alphanumeric chars and appends random digits to avoid collisions.
    /// </summary>
    private static string GenerateUsername(string name)
    {
        var baseName = new string(name.Where(c => char.IsLetterOrDigit(c)).ToArray()).ToLowerInvariant();
        if (string.IsNullOrEmpty(baseName))
            baseName = "user";

        var random = new Random();
        return $"{baseName}{random.Next(1000, 9999)}";
    }
}
