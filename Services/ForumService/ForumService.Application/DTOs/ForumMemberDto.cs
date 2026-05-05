using ForumService.ForumService.Application.Enums;

namespace ForumService.ForumService.Application.DTOs;

/// <summary>
/// Forum member profile plus role and effective permission flags for that forum.
/// </summary>
public record ForumMemberDto
{
    public Guid Id { get; init; }
    public string Username { get; init; } = string.Empty;
    public string Avatar { get; init; } = string.Empty;
    public string Bio { get; init; } = string.Empty;
    public DateTime BirthDay { get; init; }
    public int? Gender { get; init; }

    public ForumRole Role { get; init; }
    public ForumPermissionType EffectivePermissions { get; init; }
    public ForumPermissionType? PermissionOverrides { get; init; }
}
