using AuthService.AuthService.Application.Dtos;

namespace AuthService.Interfaces.Services;

public interface IOAuthService : IAuthService
{
    Task<LoginResponseDto> OAuthLogin(OAuthLoginDto dto);
}
