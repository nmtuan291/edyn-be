using ForumService.ForumService.Application.Enums;

namespace ForumService.ForumService.Application.DTOs;

/// <summary>
/// Forum member profile plus role and effective permission flags for that forum.
/// </summary>
public class ForumMemberDto
{
    public Guid Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Avatar { get; set; } = string.Empty;
    public string Bio { get; set; } = string.Empty;
    public DateTime BirthDay { get; set; }
    public int? Gender { get; set; }

    public ForumRole Role { get; set; }
    public ForumPermissionType EffectivePermissions { get; set; }
    public ForumPermissionType? PermissionOverrides { get; set; }
}
