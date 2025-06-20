using UserService.UserService.Domain.Entities;

namespace UserService.UserService.Application.Interfaces.Repositories;

public interface IUserProfileRepository
{
    Task<User?> GetUserByIdAsync(string accountId);
    Task CreateUserAsync(User user);
}