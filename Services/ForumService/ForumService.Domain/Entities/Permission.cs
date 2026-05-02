using System.ComponentModel.DataAnnotations;

namespace ForumService.ForumService.Domain.Entities;

public class Permission
{
    public Guid ForumId { get; set; }
    public Guid UserId { get; set; }
    public bool SuperModerator { get; set; }
    public bool Moderator { get; set; }
}