using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ForumService.ForumService.Application.Enums;

namespace ForumService.ForumService.Domain.Entities
{
    public class ForumUser
    {
        public Guid UserId { get; set; }
        public Guid ForumId { get; set; }
        public Forum Forum { get; set; }
        public DateTime JoinedAt { get; set; }
        public ForumRole Role { get; set; }
        public int? PermissionOverrides { get; set; }
        public bool Active { get; set; }
    }
}
