using AuthService.AuthService.Application.Dtos;
using AuthService.Entities;
using AuthService.Exceptions;
using AuthService.Interfaces.Repositories;
using AuthService.Interfaces.Services;
using AuthService.Services.Security;
using Grpc.Core;
using Microsoft.AspNetCore.Identity;
using UserService.Grpc;

namespace AuthService.Services;

public class AccountService : BaseAuthService, IAccountService
{
    public AccountService(
        UserManager<Account> userManager,
        ITokenService tokenService,
        UserProfileService.UserProfileServiceClient profileService,
        RsaKeyProvider rsaKeyProvider,
        IConfiguration config,
        ITokenRepository tokenRepository)
        : base(userManager, tokenService, profileService, rsaKeyProvider, config, tokenRepository)
    {
    }
    
    public async Task<LoginResponseDto?> Login(LoginAccountDto account)
    {
        var user = account.IsEmail ? 
            await UserManager.FindByEmailAsync(account.Username) :
            await UserManager.FindByNameAsync(account.Username);

        if (user == null)
            return null;
        
        bool isPasswordValid = await UserManager.CheckPasswordAsync(user, account.Password);
        if (!isPasswordValid)
            return null;

        return await IssueTokens(user);
    }

    public async Task<AccountDto?> GetAccount(string accountId)
    {
        var account = await UserManager.FindByIdAsync(accountId);

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
        
        var userCreated = await UserManager.CreateAsync(user, account.Password);
        if (!userCreated.Succeeded)
            return userCreated;

        try
        {
            await CreateUserProfile(user, account.Gender);
        }
        catch (RpcException ex)
        {
            await UserManager.DeleteAsync(user);
            return IdentityResult.Failed(new IdentityError() { Description = ex.Status.Detail });
        }
        catch (CreateProfileException ex)
        {
            await UserManager.DeleteAsync(user);
            return IdentityResult.Failed(new IdentityError { Description = ex.Message });
        }
        
        return IdentityResult.Success;
    }

    public async Task<bool> VerifyEmail(string email)
    {
        var user = await UserManager.FindByEmailAsync(email);
        return user != null;
    }

    public async Task<bool> VerifyUsername(string username)
    {
        var user = await UserManager.FindByNameAsync(username);
        return user != null;
    }

    public async Task<IdentityResult> ChangePassword(string accountId, ChangePasswordDto request)
    {
        if (request.NewPassword != request.NewPasswordVerify)
            return IdentityResult.Failed(new IdentityError { Code = "PasswordMismatch", Description = "New passwords do not match" });

        var user = await UserManager.FindByIdAsync(accountId);
        if (user == null)
            return IdentityResult.Failed(new IdentityError { Code = "UserNotFound", Description = "User not found" });

        var result = await UserManager.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword);
        if (result.Succeeded)
        {
            // Revoke all existing refresh tokens to force re-login on other devices
            await TokenRepository.RevokeAllRefreshTokensAsync(accountId);
        }

        return result;
    }
}