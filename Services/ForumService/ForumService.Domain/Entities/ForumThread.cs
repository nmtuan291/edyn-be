using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ForumService.ForumService.Domain.Entities
{
    public class ForumThread
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public Guid ForumId { get; set; }

        [ForeignKey("ForumId")]
        public Forum Forum { get; set; }

        [Required]
        public Guid CreatorId { get; set; }

        [Required]
        public string Title { get; set; }

        [Required]
        public bool isPinned { get; set; }

        [Required]
        public ICollection<Tag> Tags { get; set; }

        public ICollection<string> Images { get; set; }

        [Required]
        public string Content { get; set; }

        [Required]
        public string Slug { get; set; }

        [Required]
        public int Upvote { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; }

        [Required]
        public DateTime LastUpdatedAt { get; set; }
    }
}
