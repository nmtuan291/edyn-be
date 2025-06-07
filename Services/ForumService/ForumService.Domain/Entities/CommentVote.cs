using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ForumService.ForumService.Domain.Entities
{
    public class CommentVote
    {
        [Required]
        public Guid UserId { get; set; }

        [Required]
        public Guid ComentId { get; set; }

        [ForeignKey("CommentId")]
        public Comment Comment { get; set; }

        [Required]
        public bool DownVote { get; set; }
    }
}
