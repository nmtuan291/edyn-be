using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace AuthService.AuthService.Infrastructure.Data;

public class AuthDbContextFactory: IDesignTimeDbContextFactory<AuthDbContext>
{
    public AuthDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<AuthDbContext>();
        optionsBuilder.UseNpgsql("Host=localhost;Database=edyn_accountdb;Username=postgres;Password=291003");

        return new AuthDbContext(optionsBuilder.Options);
    }
}