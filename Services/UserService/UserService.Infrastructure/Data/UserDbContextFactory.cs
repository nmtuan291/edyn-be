using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace UserService.UserService.Infrastructure.Data;

public class UserDbContextFactory: IDesignTimeDbContextFactory<UserDbContext>
{
    public UserDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<UserDbContext>();
        optionsBuilder.UseNpgsql("Host=localhost;Database=edyn_userprofiledb;Username=postgres;Password=291003");

        return new UserDbContext(optionsBuilder.Options);
    }
}