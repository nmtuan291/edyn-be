using System.ComponentModel.DataAnnotations;

namespace ForumService.ForumService.Application.DTOs
{
    public record ForumDto
    {
        public Guid? Id { get; init; }

        [Required]
        [StringLength(100, MinimumLength = 3)]
        public required string Name { get; init; }

        public Guid? CreatorId { get; init; }

        [Required]
        [StringLength(30, MinimumLength = 2)]
        [RegularExpression("^[a-zA-Z0-9-_]+$", ErrorMessage = "ShortName can only contain alphanumeric characters, hyphens, and underscores.")]
        public required string ShortName { get; init; }

        [Required]
        [StringLength(1000, MinimumLength = 10)]
        public required string Description { get; init; }

        [Required]
        [Url]
        [StringLength(2048)]
        public required string ForumBanner { get; init; }

        [Required]
        [Url]
        [StringLength(2048)]
        public required string ForumImage { get; init; }

        public DateTime? CreatedAt { get; init; }
    }
}
