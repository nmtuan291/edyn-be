using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ForumService.ForumService.Domain.ValueObjects;

namespace ForumService.ForumService.Domain.Entities
{
    public class Comment
    {
        public Guid Id { get; set; }
        public Guid ThreadId { get; set; }
        public Guid OwnerId { get; set; }
        public string OwnerName { get; set; }
        public string Content { get; set; }
        public int Upvote {  get; set; }
        public Guid? ParentId { get; set; }
        public Comment? ParentComment { get; set; }
        public ICollection<Comment>? ChildrenComments { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool Deleted { get; set; }
        public ICollection<CommentVote> Votes { get; set; } = new List<CommentVote>();
        public Comment() { }

        public Comment(CommentCreation creation)
        {
            if (creation.ThreadId == Guid.Empty)
                throw new ArgumentException("ThreadId cannot be empty");
            if (creation.OwnerId == Guid.Empty)
                throw new ArgumentException("OwnerId cannot be empty");
            
            Id = Guid.NewGuid();
            ThreadId = creation.ThreadId;
            OwnerId = creation.OwnerId;
            OwnerName = creation.OwnerName;
            Content = creation.Content;
            Upvote = 0;
            ParentId = creation.ParentId is null || creation.ParentId.Value == Guid.Empty
                ? null
                : creation.ParentId;
            ChildrenComments = new List<Comment>();
            UpdatedAt = DateTime.UtcNow;
            CreatedAt = DateTime.UtcNow;
            Deleted = false;
        }
        
        public bool Vote(Guid userId, bool isDownVote)
        {
            bool voteExists = false;
            var vote = Votes.FirstOrDefault(x => x.UserId == userId);
            if (vote == null)
            {
                vote = new CommentVote()
                {
                    CommentId = Id,
                    UserId = userId,
                    DownVote = isDownVote,
                };
                Votes.Add(vote);
                voteExists = true;
            }
            else
            {
                if (isDownVote == vote.DownVote)
                {
                    Votes.Remove(vote);
                }
                else
                {
                    vote.DownVote = isDownVote;
                    voteExists = true;
                }
            }

            int upVote = Votes.Count(v => !v.DownVote);
            int downVote = Votes.Count(v => v.DownVote);
            Upvote = upVote -  downVote;
            
            return voteExists;
        }
    }
}
