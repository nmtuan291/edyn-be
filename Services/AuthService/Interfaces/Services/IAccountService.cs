using AuthService.AuthService.Application.Dtos;
using Microsoft.AspNetCore.Identity;

namespace AuthService.Interfaces.Services;

public interface IAccountService
{
    Task<LoginResponseDto?> Login(LoginAccountDto account);
    Task<AccountDto?> GetAccount(string accountId);
    Task<IdentityResult> Register(RegisterAccountDto account);
    Task<bool> VerifyEmail(string email);
    Task<bool> VerifyUsername(string username);
    Task<TokenResponse> RefreshToken(string accountId, string refreshToken);
    Task Logout(string accountId, string refreshToken);
    Task<IdentityResult> ChangePassword(string accountId, ChangePasswordDto request);
    Task RevokeAllSessions(string accountId);
}