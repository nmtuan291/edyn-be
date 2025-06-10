using AuthService.AuthService.Domain.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace AuthService.AuthService.Infrastructure.Data;

public class AuthDbContext: IdentityDbContext<Account>
{
    public AuthDbContext(DbContextOptions<AuthDbContext> options): base(options) { }
}