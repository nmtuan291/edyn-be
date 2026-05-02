namespace ForumService.ForumService.Application.DTOs;

public class ForumTagDto
{
    public int Id { get; set; }
    public Guid ForumId { get; set; }
    public required string Name { get; set; }
    public required string Color { get; set; }
}
