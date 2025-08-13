using ChatService.ChatService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ChatService.ChatService.Infrastructure.Data;

public class ChatDbContext: DbContext
{
    public ChatDbContext(DbContextOptions<ChatDbContext> options) : base(options) { }
    
    public DbSet<Conversation> Conversations { get; set; }
    public DbSet<Message> Messages { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Conversation>(entity =>
        {
            entity.HasKey(c => c.Id);
            
            entity.HasOne(c => c.LastMessage)
                  .WithMany()
                  .HasForeignKey(c => c.LastMessageId)
                  .OnDelete(DeleteBehavior.SetNull);
                  
            entity.HasIndex(c => new { c.User1Id, c.User2Id })
                  .HasDatabaseName("IX_Conversations_Users");
        });

        modelBuilder.Entity<Message>(entity =>
        {
            entity.HasKey(m => m.Id);
            
            entity.HasOne(m => m.Conversation)
                  .WithMany(c => c.Messages)
                  .HasForeignKey(m => m.ConversationId)
                  .OnDelete(DeleteBehavior.Cascade);
                  
            entity.HasIndex(m => new { m.ConversationId, m.CreatedAt })
                  .HasDatabaseName("IX_Messages_ConversationId_CreatedAt");
                  
            entity.HasIndex(m => new { m.ReceiverId, m.IsRead, m.CreatedAt })
                  .HasDatabaseName("IX_Messages_Unread");
        });
    }
}