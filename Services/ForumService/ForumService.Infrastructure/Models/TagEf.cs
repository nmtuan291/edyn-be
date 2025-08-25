using System.ComponentModel.DataAnnotations;

namespace ForumService.ForumService.Infrastructure.Models
{
    public class TagEf
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public string Color { get; set; }
    }
}
