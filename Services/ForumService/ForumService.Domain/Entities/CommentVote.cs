using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ForumService.ForumService.Domain.Entities
{
    public class CommentVote
    {
        public Guid UserId { get; set; }
        public Guid CommentId { get; set; }
        public Comment Comment { get; set; }
        public bool DownVote { get; set; }
    }
}
