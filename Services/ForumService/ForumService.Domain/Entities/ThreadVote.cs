using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ForumService.ForumService.Domain.Entities
{
    public class ThreadVote
    {
        public Guid UserId { get; set; }
        public Guid ThreadId { get; set; }
        public ForumThread Thread { get; set; }
        public bool DownVote { get; set; }
    }
}
