using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ForumService.ForumService.Infrastructure.Models
{
    public class CommentVoteEf
    {
        [Required]
        public Guid UserId { get; set; }

        [Required]
        public Guid ComentId { get; set; }

        [ForeignKey("CommentId")]
        public CommentEf CommentEf { get; set; }

        [Required]
        public bool DownVote { get; set; }
    }
}
