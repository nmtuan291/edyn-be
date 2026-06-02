using System.ComponentModel.DataAnnotations;

namespace ForumService.ForumService.Application.Requests;

public record EditCommentRequest
{
    [Required]
    [StringLength(10000, MinimumLength = 1)]
    public required string Content { get; init; }
}
