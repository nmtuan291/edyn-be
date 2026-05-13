using AuthService.AuthService.Application.Dtos;
using Microsoft.AspNetCore.Identity;

namespace AuthService.Interfaces.Services;

public interface IAccountService : IAuthService
{
    Task<LoginResponseDto?> Login(LoginAccountDto account);
    Task<AccountDto?> GetAccount(string accountId);
    Task<IdentityResult> Register(RegisterAccountDto account);
    Task<bool> VerifyEmail(string email);
    Task<bool> VerifyUsername(string username);
    Task<IdentityResult> ChangePassword(string accountId, ChangePasswordDto request);
}