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
        if (user == null)
            return null;

        return new UserProfileDto
        {
            AccountId = accountId,
            Avatar = user.Avatar,
            Birthday = user.Birthday ?? DateTime.MinValue,
            UserName = user.Username,
            Gender = user.Gender,
        };
    }

    public async Task<List<UserProfileDto>> GetUsers(IEnumerable<string> accountIds)
    {
        var users = await _userProfileRepository.GetUsersByIdsAsync(accountIds);
        return users.Select(u => new UserProfileDto
        {
            AccountId = u.AccountId,
            Avatar = u.Avatar,
            Birthday = u.Birthday ?? DateTime.MinValue,
            UserName = u.Username,
            Gender = u.Gender,
        }).ToList();
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
