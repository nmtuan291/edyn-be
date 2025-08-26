using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ForumService.ForumService.Domain.Entities
{
    public class ForumThread
    {
        public Guid Id { get; set; }
        public Guid ForumId { get; set; }
        public Guid CreatorId { get; set; }
        public string Title { get; set; }
        public bool IsPinned { get; set; }
        public ICollection<Tag> Tags { get; set; }
        public ICollection<string>? Images { get; set; }
        public ICollection<Poll>? PollItems { get; set; }
        public string Content { get; set; }
        public string Slug { get; set; }
        public int Upvote { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime LastUpdatedAt { get; set; }
        public ICollection<ThreadVote> Votes { get; set; }
        
        public bool Vote(Guid userId, bool isDownVote)
        {
            bool voteExists = false;
            var vote = Votes.FirstOrDefault(x => x.UserId == userId);
            if (vote == null)
            {
                vote = new ThreadVote()
                {
                    ThreadId = Id,
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
