using AuthService.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Data;

public class AuthDbContext: IdentityDbContext<Account>
{
    public AuthDbContext(DbContextOptions<AuthDbContext> options): base(options) { }
}