using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ForumService.ForumService.Infrastructure.Models
{
    public class ForumThreadEf
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public Guid ForumId { get; set; }

        [ForeignKey("ForumId")]
        public ForumEf ForumEf { get; set; }

        [Required]
        public Guid CreatorId { get; set; }

        [Required]
        [StringLength(300)]
        public string Title { get; set; }

        [Required]
        public bool IsPinned { get; set; }

        [Required]
        public ICollection<TagEf> Tags { get; set; }

        public ICollection<string>? Images { get; set; }
        
        public ICollection<PollEf>? PollItems { get; set; }

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
        
        public ICollection<ThreadVoteEf> Votes { get; set; }
    }
}
