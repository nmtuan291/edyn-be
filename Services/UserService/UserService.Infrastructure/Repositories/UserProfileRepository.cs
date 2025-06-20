using Microsoft.EntityFrameworkCore;
using UserService.UserService.Application.Interfaces.Repositories;
using UserService.UserService.Domain.Entities;
using UserService.UserService.Infrastructure.Data;

namespace UserService.UserService.Infrastructure.Repositories;

public class UserProfileRepository : IUserProfileRepository
{
    private readonly UserDbContext _context;

    public UserProfileRepository(UserDbContext context)
    {
        _context = context;
    }

    public async Task<User?> GetUserByIdAsync(string accountId)
    {
        var user = await _context.Users.SingleOrDefaultAsync(u => u.AccountId == accountId);
        
        return user;
    }

    public async Task CreateUserAsync(User user)
    {
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();
    }
}