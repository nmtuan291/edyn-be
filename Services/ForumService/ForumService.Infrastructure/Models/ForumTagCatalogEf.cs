using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ForumService.ForumService.Infrastructure.Models;

public class ForumTagCatalogEf
{
    [Key]
    public int Id { get; set; }

    [Required]
    public Guid ForumId { get; set; }

    [ForeignKey(nameof(ForumId))]
    public ForumEf ForumEf { get; set; }

    [Required]
    [MaxLength(64)]
    public string Name { get; set; }

    [Required]
    [MaxLength(32)]
    public string Color { get; set; }
}
