namespace ForumService.ForumService.Domain.ValueObjects;

public record CommentCreation(
    Guid ThreadId,
    Guid OwnerId,
    string OwnerName,
    string Content,
    Guid? ParentId);
