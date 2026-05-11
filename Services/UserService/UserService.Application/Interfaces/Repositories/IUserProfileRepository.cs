using UserService.UserService.Domain.Entities;

namespace UserService.UserService.Application.Interfaces.Repositories;

public interface IUserProfileRepository
{
    Task<User?> GetUserByIdAsync(string accountId);
    Task CreateUserAsync(User user);
    Task<List<User>> GetUsersByIdsAsync(IEnumerable<string> accountIds);
    Task UpdateUserAsync(User user);
}