using ForumService.ForumService.Domain.Entities;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using ForumService.ForumService.Application.Enums;

namespace ForumService.ForumService.Application.DTOs
{
    public record ForumThreadDto
    {
        public Guid? Id { get; init; }
        public required Guid ForumId { get; init; }
        public Guid? CreatorId { get; init; }
        public required string Title { get; init; }
        public required bool IsPinned { get; init; }
        public ICollection<Tag>? Tags { get; init; }
        public ICollection<PollItemDto>? PollItems { get; init; }
        public ICollection<string>? Images { get; init; }
        public required string Content { get; init; }
        public required string Slug { get; init; }
        public required int Upvote { get; init; }
        public  DateTime? CreatedAt { get; init; }
        public DateTime? LastUpdatedAt { get; init; }
        public VoteStatus Vote { get; init; }
        public string? ForumName { get; init; }
        public string? ForumImage { get; init; }
    }
}
