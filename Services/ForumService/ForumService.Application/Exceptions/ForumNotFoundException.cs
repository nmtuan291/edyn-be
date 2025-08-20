namespace ForumService.ForumService.Application.Exceptions;

public class ForumNotFoundException : Exception
{
    public ForumNotFoundException(Guid forumId) : base($"Forum with id {forumId} not found") {}
}