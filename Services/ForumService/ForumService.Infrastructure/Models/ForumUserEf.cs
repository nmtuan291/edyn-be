using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ForumService.ForumService.Infrastructure.Models
{
    public class ForumUserEf
    {
        [Required]
        public Guid UserId { get; set; }

        [Required]
        public Guid ForumId { get; set; }

        [ForeignKey("ForumId")]
        public ForumEf ForumEf { get; set; }

        [Required]
        public DateTime JoinedAt { get; set; }

        [Required]
        public bool Moderator {  get; set; }

        [Required]
        public bool Active { get; set; }
    }
}
