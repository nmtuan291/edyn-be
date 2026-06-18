using System.ComponentModel.DataAnnotations;

namespace ForumService.ForumService.Application.Requests;

public record EditThreadRequest
{
    [StringLength(200, MinimumLength = 3)]
    public string? Title { get; init; }

    [StringLength(50000, MinimumLength = 1)]
    public string? Content { get; init; }

    public ICollection<string>? Videos { get; init; }
}
