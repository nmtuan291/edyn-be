using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ForumService.ForumService.Infrastructure.Models
{
    public class ThreadVoteEf
    {
        [Required]
        public Guid UserId { get; set; }

        [Required]
        public Guid ThreadId { get; set; }

        [ForeignKey("ThreadId")]
        public ForumThreadEf ThreadEf { get; set; }

        [Required]
        public bool DownVote { get; set; }
    }
}
