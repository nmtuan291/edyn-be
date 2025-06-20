using Microsoft.EntityFrameworkCore;
using UserService.UserService.Domain.Entities;

namespace UserService.UserService.Infrastructure.Data;

public class UserDbContext: DbContext
{
    public UserDbContext(DbContextOptions<UserDbContext> options): base(options) { }
    
    public DbSet<User> Users { get; set; }
} 