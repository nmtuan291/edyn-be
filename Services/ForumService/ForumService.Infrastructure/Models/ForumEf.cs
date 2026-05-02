using System.ComponentModel.DataAnnotations;

namespace ForumService.ForumService.Infrastructure.Models
{
    public class ForumEf
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public string ShortName { get; set; }

        [Required]
        public Guid CreatorId { get; set; }

        [Required]
        public string Description { get; set; }

        public string ForumBanner { get; set; }

        public string ForumImage { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; }
        
    }
}
