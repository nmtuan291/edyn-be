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
                .HasKey(v => new { v.UserId, v.CommentId });
            modelBuilder.Entity<ThreadVoteEf>()
                .HasKey(v => new { v.UserId, v.ThreadId });

            modelBuilder.Entity<ForumUserEf>()
                .HasKey(t => new { t.UserId, t.ForumId });
            modelBuilder.Entity<PollEf>()
                .HasKey(p => new { p.ThreadId, p.PollContent });
            modelBuilder.Entity<ForumEf>()
                .HasIndex(f => f.Name)
                .IsUnique();

            modelBuilder.Entity<ForumTagCatalogEf>(e =>
            {
                e.ToTable("ForumTagCatalog");
                e.HasIndex(x => new { x.ForumId, x.Name }).IsUnique();
                e.HasOne(x => x.ForumEf)
                    .WithMany()
                    .HasForeignKey(x => x.ForumId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
            
            modelBuilder.Entity<PollEf>().ToTable("Poll");
            
            modelBuilder.Entity<OutboxMessageEf>(e =>
            {
                e.ToTable("outbox");
                e.HasKey(x => x.Id);
                e.Property(x => x.AggregateType).HasColumnName("aggregate_type").HasMaxLength(255);
                e.Property(x => x.AggregateId).HasColumnName("aggregate_id").HasMaxLength(255);
                e.Property(x => x.EventType).HasColumnName("event_type").HasMaxLength(255);
                e.Property(x => x.Payload).HasColumnName("payload").HasColumnType("jsonb");
                e.Property(x => x.CreatedAt).HasColumnName("created_at");
            });
            
            base.OnModelCreating(modelBuilder);
        }

        public DbSet<ForumEf> Forums { get; set; }
        public DbSet<ForumThreadEf> Threads { get; set; }
        public DbSet<CommentEf> Comments { get; set; }
        public DbSet<TagEf> Tags { get; set; }
        public DbSet<ForumTagCatalogEf> ForumTagCatalog { get; set; }
        public DbSet<ForumUserEf> ForumUsers { get; set; }
        public DbSet<ThreadVoteEf> ThreadVotes { get; set; }
        public DbSet<CommentVoteEf> CommentVotes { get; set; }
        public DbSet<OutboxMessageEf> OutboxMessages { get; set; }
    }
}
