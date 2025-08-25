using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ForumService.ForumService.Infrastructure.Models
{
    public class CommentEf
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public Guid ThreadId { get; set; }

        [ForeignKey("ThreadId")]
        public ForumThreadEf ThreadEf { get; set; }

        [Required]
        public Guid OwnerId { get; set; }
        
        [Required]
        public string OwnerName { get; set; }

        [Required]
        [StringLength(500)]
        public string Content { get; set; }

        [Required]
        public int Upvote {  get; set; }

        public Guid? ParentId { get; set; }

        [ForeignKey("ParentId")]
        public CommentEf? ParentComment { get; set; }

        public ICollection<CommentEf>? ChildrenComments { get; set; }

        public DateTime? UpdatedAt { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; }

        [Required]
        public bool Deleted { get; set; }
        
        public CommentEf() { }

        public CommentEf(Guid threadId, Guid ownerId, string ownerName,string content, Guid parentId)
        {
            if (threadId == Guid.Empty)
                throw new ArgumentException("ThreadId cannot be empty");
            if (ownerId == Guid.Empty)
                throw new ArgumentException("OwnerId cannot be empty");
            if (string.IsNullOrEmpty(ownerName))
                throw new ArgumentException("OwnerName cannot be empty");
            
            Id = Guid.NewGuid();
            ThreadId = threadId;
            OwnerId = ownerId;
            OwnerName = ownerName;
            Content = content;
            Upvote = 0;
            ParentId = parentId;
            ChildrenComments = new List<CommentEf>();
            UpdatedAt = DateTime.UtcNow;
            CreatedAt = DateTime.UtcNow;
            Deleted = false;
        }
    }
}
