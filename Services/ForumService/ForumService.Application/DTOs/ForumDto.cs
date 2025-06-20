using System.ComponentModel.DataAnnotations;

namespace ForumService.ForumService.Application.DTOs
{
    public class ForumDto
    {
        public required Guid? Id { get; set; }
        public required string Name { get; set; }
        public required Guid CreatorId { get; set; }
        public required string ShortName { get; set; }
        public required string Description { get; set; }
        public required string ForumBanner { get; set; }
        public required string ForumImage { get; set; }
        public required DateTime CreatedAt { get; set; }
    }
}
