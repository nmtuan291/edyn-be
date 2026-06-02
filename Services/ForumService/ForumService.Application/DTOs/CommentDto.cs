using ForumService.ForumService.Domain.Entities;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using ForumService.ForumService.Application.Enums;
namespace ForumService.ForumService.Application.DTOs
{
    public record CommentDto
    {
        public Guid? Id { get; init; }

        [Required]
        public required Guid ThreadId { get; init; }

        public Guid? OwnerId { get; init; }
        public string? OwnerName { get; init; }

        [Required]
        [StringLength(10000, MinimumLength = 1)]
        public required string Content { get; init; }

        [Required]
        [Range(0, int.MaxValue)]
        public required int Upvote { get; init; }

        public Guid? ParentId { get; init; }
        public ICollection<CommentDto>? ChildrenComments { get; init; }
        public DateTime? UpdatedAt { get; init; }
        public DateTime? CreatedAt { get; init; }

        [Required]
        public required bool Deleted { get; init; }

        public VoteStatus Vote { get; set; }
    }
}
