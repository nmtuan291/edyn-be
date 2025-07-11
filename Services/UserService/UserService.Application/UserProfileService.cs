using UserService.Grpc;
using UserService.UserService.Application.Dtos;
using UserService.UserService.Application.Interfaces.Repositories;
using UserService.UserService.Application.Interfaces.Services;
using UserService.UserService.Domain.Entities;

namespace UserService.UserService.Application;

public class UserProfileService : IUserProfileService
{
    private readonly IUserProfileRepository _userProfileRepository;

    public UserProfileService(IUserProfileRepository userProfileRepository)
    {
        _userProfileRepository = userProfileRepository;
    }

    public async Task<UserProfileDto?> GetUserById(string accountId)
    {
        if (string.IsNullOrEmpty(accountId))
            throw new ArgumentNullException(nameof(accountId));

        var user = await _userProfileRepository.GetUserByIdAsync(accountId);

        return null;
    }

    public async Task CreateProfile(CreateProfileRequest request)
    {
        User user = new()
        {
             AccountId = request.Id,
             Avatar = "",
             Username = request.Username,
             Gender = request.Gender
        };

        await _userProfileRepository.CreateUserAsync(user);
    }
}
