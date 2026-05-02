using ForumService.ForumService.Application.Enums;

namespace ForumService.ForumService.Application.DTOs;

public class ForumUserDto
{
    public Guid ForumId { get; set; }
    public string Name { get; set; }
    public string ForumImage { get; set; }
    public ForumRole Role { get; set; }
}