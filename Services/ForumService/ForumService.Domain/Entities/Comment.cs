using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ForumService.ForumService.Domain.Entities
{
    public class Comment
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public Guid ThreadId { get; set; }

        [ForeignKey("ThreadId")]
        public ForumThread Thread { get; set; }

        [Required]
        public Guid OwnerId { get; set; }

        [Required]
        [StringLength(500)]
        public string Content { get; set; }

        [Required]
        public int Upvate {  get; set; }

        public Guid ParentId { get; set; }

        [ForeignKey("ParentId")]
        public Comment ParmentComment { get; set; }

        public ICollection<Comment> ChildrenComments { get; set; }

        public DateTime UpdatedAt { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; }

        [Required]
        public bool Deleted { get; set; }
    }
}
