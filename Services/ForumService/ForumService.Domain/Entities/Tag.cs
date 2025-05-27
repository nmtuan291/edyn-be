using System.ComponentModel.DataAnnotations;

namespace ForumService.ForumService.Domain.Entities
{
    public class Tag
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public string Color { get; set; }
    }
}
