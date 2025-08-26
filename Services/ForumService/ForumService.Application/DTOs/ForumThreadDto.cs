using ForumService.ForumService.Domain.Entities;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using ForumService.ForumService.Application.Enums;

namespace ForumService.ForumService.Application.DTOs
{
    public class ForumThreadDto
    {
        public Guid? Id { get; set; }
        public required Guid ForumId { get; set; }
        public Guid? CreatorId { get; set; }
        public required string Title { get; set; }
        public required bool IsPinned { get; set; }
        public ICollection<Tag>? Tags { get; set; }
        public ICollection<PollItemDto>? PollItems { get; set; }
        public ICollection<string>? Images { get; set; }
        public required string Content { get; set; }
        public required string Slug { get; set; }
        public required int Upvote { get; set; }
        public  DateTime? CreatedAt { get; set; }
        public DateTime? LastUpdatedAt { get; set; }
        public VoteStatus Vote { get; set; }
    }
}
