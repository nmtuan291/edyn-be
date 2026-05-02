using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ForumService.ForumService.Application.Enums;

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
        public ForumRole Role { get; set; }

        public int? PermissionOverrides { get; set; }

        [Required]
        public bool Active { get; set; }
    }
}
