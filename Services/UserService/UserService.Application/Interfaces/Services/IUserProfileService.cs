using UserService.Grpc;
using UserService.UserService.Application.Dtos;
using UserService.UserService.Domain.Entities;

namespace UserService.UserService.Application.Interfaces.Services;

public interface IUserProfileService
{
    Task<UserProfileDto?> GetUserById(string accountId);
    Task CreateProfile(CreateProfileRequest request);
    Task<List<UserProfileDto>> GetUsers(IEnumerable<string> accountIds);
}