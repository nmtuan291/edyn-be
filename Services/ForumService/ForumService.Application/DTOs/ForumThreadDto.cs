using ForumService.ForumService.Domain.Entities;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using ForumService.ForumService.Application.Enums;

namespace ForumService.ForumService.Application.DTOs
{
    public record ForumThreadDto
    {
        public Guid? Id { get; init; }

        [Required]
        public required Guid ForumId { get; init; }

        public Guid? CreatorId { get; init; }

        [Required]
        [StringLength(200, MinimumLength = 3)]
        public required string Title { get; init; }

        [Required]
        public required bool IsPinned { get; init; }

        public ICollection<Tag>? Tags { get; init; }
        public ICollection<PollItemDto>? PollItems { get; init; }
        public ICollection<string>? Images { get; init; }
        public ICollection<string>? Videos { get; init; }

        [Required]
        [StringLength(50000, MinimumLength = 1)]
        public required string Content { get; init; }

        [Required]
        [StringLength(250)]
        public required string Slug { get; init; }

        [Required]
        [Range(0, int.MaxValue)]
        public required int Upvote { get; init; }

        public  DateTime? CreatedAt { get; init; }
        public DateTime? LastUpdatedAt { get; init; }
        public VoteStatus Vote { get; set; }
        public string? UserPollVote { get; set; }
        public string? ForumName { get; set; }
        public string? ForumImage { get; set; }
        public string? CreatorName { get; set; }
        public string? CreatorAvatar { get; set; }
    }
}
