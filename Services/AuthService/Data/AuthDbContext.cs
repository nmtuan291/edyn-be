using AuthService.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Data;

public class AuthDbContext: IdentityDbContext<Account>
{
    public DbSet<RefreshToken> RefreshTokens { get; set; }
    public AuthDbContext(DbContextOptions<AuthDbContext> options): base(options) { }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.Entity<RefreshToken>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Token).IsRequired();
            entity.Property(x => x.Expires).IsRequired();
            entity.HasOne(x => x.Account);
        });
        
        builder.Entity<Account>()
            .HasMany(a => a.RefreshTokens)
            .WithOne(r => r.Account)
            .HasForeignKey(r => r.AccountId);
        
        base.OnModelCreating(builder);
    }
}