using System.ComponentModel.DataAnnotations;

namespace ForumService.ForumService.Application.Requests;

public record CreateForumTagRequest
{
    [Required]
    [StringLength(50, MinimumLength = 1)]
    public required string Name { get; init; }

    [StringLength(7)]
    [RegularExpression("^#([A-Fa-f0-9]{6}|[A-Fa-f0-9]{3})$", ErrorMessage = "Color must be a valid hex color code (e.g. #FFFFFF or #FFF)")]
    public string? Color { get; init; }
}
