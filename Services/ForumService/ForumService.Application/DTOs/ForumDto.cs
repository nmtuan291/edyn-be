using System.ComponentModel.DataAnnotations;

namespace ForumService.ForumService.Application.DTOs
{
    public record ForumDto
    {
        public Guid? Id { get; init; }
        public required string Name { get; init; }
        public Guid? CreatorId { get; init; }
        public required string ShortName { get; init; }
        public required string Description { get; init; }
        public required string ForumBanner { get; init; }
        public required string ForumImage { get; init; }
        public DateTime? CreatedAt { get; init; }
    }
}
