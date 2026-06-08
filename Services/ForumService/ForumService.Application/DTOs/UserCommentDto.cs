
namespace ForumService.ForumService.Application.DTOs
{
    public record UserCommentDto : CommentDto
    {
        public required string ThreadTitle { get; init; }
        public required string RealmShortName { get; init; }
    }
}
