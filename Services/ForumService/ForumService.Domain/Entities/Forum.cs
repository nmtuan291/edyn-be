using System.ComponentModel.DataAnnotations;

namespace ForumService.ForumService.Domain.Entities
{
    public class Forum
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public Guid CreatorId { get; set; }

        [Required]
        public string Description { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; }
    }
}
