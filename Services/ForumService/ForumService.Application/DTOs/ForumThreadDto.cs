using ForumService.ForumService.Domain.Entities;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace ForumService.ForumService.Application.DTOs
{
    public class ForumThreadDto
    {
        public required Guid Id { get; set; }
        public required Guid ForumId { get; set; }
        public required Guid CreatorId { get; set; }
        public required string Title { get; set; }
        public required bool IsPinned { get; set; }
        public required ICollection<Tag> Tags { get; set; }
        public ICollection<string>? Images { get; set; }
        public required string Content { get; set; }
        public required string Slug { get; set; }
        public required int Upvote { get; set; }
        public required DateTime CreatedAt { get; set; }
        public DateTime? LastUpdatedAt { get; set; }
    }
}
