using AuthService.AuthService.Application.Dtos;
using Microsoft.AspNetCore.Identity;

namespace AuthService.AuthService.Application.Interfaces.Services;

public interface IAccountService
{
    Task<LoginResponseDto?> Login(LoginAccountDto account);
    Task<AccountDto> GetAccount(string accountId);
    Task<IdentityResult> Register(RegisterAccountDto account);

}