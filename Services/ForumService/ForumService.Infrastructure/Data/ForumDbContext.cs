using ForumService.ForumService.Domain.Entities;
using ForumService.ForumService.Infrastructure.Models;
using Microsoft.EntityFrameworkCore;

namespace ForumService.ForumService.Infrastructure.Data
{
    public class ForumDbContext: DbContext
    {
        public ForumDbContext(DbContextOptions<ForumDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<CommentVoteEf>()
                .HasKey(v => new { v.UserId, v.ComentId });
            modelBuilder.Entity<ThreadVoteEf>()
                .HasKey(v => new { v.UserId, v.ThreadId });

            modelBuilder.Entity<ForumUserEf>()
                .HasKey(t => new { t.UserId, t.ForumId });
            modelBuilder.Entity<PollEf>()
                .HasKey(p => new { p.ThreadId, p.PollContent });
            modelBuilder.Entity<ForumEf>()
                .HasIndex(f => f.Name)
                .IsUnique();
            
            modelBuilder.Entity<PollEf>().ToTable("Poll");
            
            base.OnModelCreating(modelBuilder);
        }

        public DbSet<ForumEf> Forums { get; set; }
        public DbSet<ForumThreadEf> Threads { get; set; }
        public DbSet<CommentEf> Comments { get; set; }
        public DbSet<TagEf> Tags { get; set; }
        public DbSet<ForumUserEf> ForumUsers { get; set; }
        public DbSet<ThreadVoteEf> ThreadVotes { get; set; }
        public DbSet<CommentVoteEf> CommentVotes { get; set; }
    }
}
