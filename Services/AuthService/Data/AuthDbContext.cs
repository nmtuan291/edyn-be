using AuthService.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Data;

public class AuthDbContext: IdentityDbContext<Account>
{
    public DbSet<RefreshToken> RefreshTokens { get; set; }
    public DbSet<OAuthLogin> OAuthLogins { get; set; }
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

        builder.Entity<OAuthLogin>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Provider).IsRequired().HasMaxLength(50);
            entity.Property(x => x.ProviderUserId).IsRequired().HasMaxLength(256);
            entity.HasIndex(x => new { x.Provider, x.ProviderUserId }).IsUnique();
            entity.HasOne(x => x.Account)
                .WithMany(a => a.OAuthLogins)
                .HasForeignKey(x => x.AccountId);
        });
        
        base.OnModelCreating(builder);
    }
}