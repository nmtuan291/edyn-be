using ForumService.ForumService.Domain.Entities;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace ForumService.ForumService.Application.DTOs
{
    public class ForumThreadDto
    {
        [Required]
        public Guid Id { get; set; }

        [Required]
        public Guid ForumId { get; set; }

        [Required]
        public Guid CreatorId { get; set; }

        [Required]
        public string Title { get; set; }

        [Required]
        public bool IsPinned { get; set; }

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
