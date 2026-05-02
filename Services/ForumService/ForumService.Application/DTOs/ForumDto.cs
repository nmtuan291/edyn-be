using System.ComponentModel.DataAnnotations;

namespace ForumService.ForumService.Application.DTOs
{
    public class ForumDto
    {
        public Guid? Id { get; set; }
        public required string Name { get; set; }
        public Guid? CreatorId { get; set; }
        public required string ShortName { get; set; }
        public required string Description { get; set; }
        public required string ForumBanner { get; set; }
        public required string ForumImage { get; set; }
        public DateTime? CreatedAt { get; set; }
    }
}
