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
    }
}
