using UserService.Grpc;
using UserService.UserService.Application.Dtos;

namespace UserService.UserService.Application.Interfaces.Services;

public interface IUserProfileService
{
    Task<UserProfileDto?> GetUserById(string accountId);
    Task CreateProfile(CreateProfileRequest request);
}