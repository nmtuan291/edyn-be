using AuthService.AuthService.Application.Interfaces.Repositories;
using AuthService.AuthService.Domain.Entities;
using AuthService.AuthService.Infrastructure.Data;

namespace AuthService.AuthService.Infrastructure.Repositories;

public class AccountRepository : IAccountRepository
{
    private readonly AuthDbContext _context;

    public AccountRepository(AuthDbContext context)
    {
        _context = context;
    }
}