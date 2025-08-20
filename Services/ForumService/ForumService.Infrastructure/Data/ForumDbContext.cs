using ForumService.ForumService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ForumService.ForumService.Infrastructure.Data
{
    public class ForumDbContext: DbContext
    {
        public ForumDbContext(DbContextOptions<ForumDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            modelBuilder.Entity<CommentVote>()
                .HasKey(v => new { v.UserId, v.ComentId });
            modelBuilder.Entity<ThreadVote>()
                .HasKey(v => new { v.UserId, v.ThreadId });

            modelBuilder.Entity<ForumUser>()
                .HasKey(t => new { t.UserId, t.ForumId });
            modelBuilder.Entity<Poll>()
                .HasKey(p => new { p.ThreadId, p.PollContent });
            modelBuilder.Entity<Forum>()
                .HasIndex(f => f.Name)
                .IsUnique();
        }

        public DbSet<Forum> Forums { get; set; }
        public DbSet<ForumThread> Threads { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<Tag> Tags { get; set; }
        public DbSet<ForumUser> ForumUsers { get; set; }
        public DbSet<ThreadVote> ThreadVotes { get; set; }
        public DbSet<CommentVote> CommentVotes { get; set; }
    }
}
