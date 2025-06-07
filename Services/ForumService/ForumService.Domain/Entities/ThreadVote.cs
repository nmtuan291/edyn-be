using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ForumService.ForumService.Domain.Entities
{
    public class ThreadVote
    {
        [Required]
        public Guid UserId { get; set; }

        [Required]
        public Guid ThreadId { get; set; }

        [ForeignKey("ThreadId")]
        public ForumThread Thread { get; set; }

        [Required]
        public bool DownVote { get; set; }
    }
}
