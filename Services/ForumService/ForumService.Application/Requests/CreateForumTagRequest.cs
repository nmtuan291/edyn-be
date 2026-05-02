namespace ForumService.ForumService.Application.Requests;

public class CreateForumTagRequest
{
    public required string Name { get; set; }
    public string? Color { get; set; }
}
