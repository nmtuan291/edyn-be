using ForumService.ForumService.Domain.Entities;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace ForumService.ForumService.Application.DTOs
{
    public class CommentDto
    {
        public Guid? Id { get; set; }

        [Required]
        public Guid ThreadId { get; set; }

        [Required]
        public Guid OwnerId { get; set; }

        [Required]
        [StringLength(500)]
        public string Content { get; set; }

        [Required]
        public int Upvote { get; set; }

        public Guid? ParentId { get; set; }

        public ICollection<Comment>? ChildrenComments { get; set; }

        public DateTime? UpdatedAt { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; }

        [Required]
        public bool Deleted { get; set; }
    }
}
