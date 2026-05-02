using System.Text.Json.Serialization;

namespace ForumService.ForumService.Application.Requests;

public class VoteRequest
{
    /// <summary>Thread or comment id (works for both vote endpoints).</summary>
    public Guid Id { get; set; }

    [JsonPropertyName("threadId")]
    public Guid? ThreadId { get; set; }

    [JsonPropertyName("commentId")]
    public Guid? CommentId { get; set; }

    public bool IsDownvote { get; set; }

    /// <summary>Optional numeric vote: 1 = upvote, -1 = downvote (overrides <see cref="IsDownvote"/> when set).</summary>
    [JsonPropertyName("vote")]
    public int? Vote { get; set; }

    public Guid GetThreadId()
    {
        if (Id != Guid.Empty)
            return Id;
        return ThreadId is { } t && t != Guid.Empty ? t : Guid.Empty;
    }

    public Guid GetCommentId()
    {
        if (Id != Guid.Empty)
            return Id;
        return CommentId is { } c && c != Guid.Empty ? c : Guid.Empty;
    }

    public bool GetIsDownvote()
    {
        if (Vote.HasValue)
            return Vote.Value < 0;
        return IsDownvote;
    }
}
