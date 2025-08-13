using System.ComponentModel.DataAnnotations;

namespace ForumService.ForumService.Domain.Entities;

public class Permission
{
    [Required]
    public Guid ForumId { get; set; }
    [Required]
    public Guid UserId { get; set; }
    [Required]
    public bool SuperModerator { get; set; }
    [Required]
    public bool Moderator { get; set; }
}