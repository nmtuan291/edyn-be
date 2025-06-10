using AuthService.AuthService.Application.Dtos;

namespace AuthService.AuthService.Application.Interfaces.Services;

public interface IAccountService
{
    Task<LoginResponseDto?> Login(LoginAccountDto account);
}