using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ForumService.ForumService.Domain.Entities
{
    public class ForumThread
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public Guid ForumId { get; set; }

        [ForeignKey("ForumId")]
        public Forum Forum { get; set; }

        [Required]
        public Guid CreatorId { get; set; }

        [Required]
        [StringLength(300)]
        public string Title { get; set; }

        [Required]
        public bool IsPinned { get; set; }

        [Required]
        public ICollection<Tag> Tags { get; set; }

        public ICollection<string>? Images { get; set; }
        
        public ICollection<Poll>? PollItems { get; set; }

        [Required]
        public string Content { get; set; }

        [Required]
        public string Slug { get; set; }

        [Required]
        public int Upvote { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; }

        [Required]
        public DateTime LastUpdatedAt { get; set; }
        
        public ICollection<ThreadVote> Votes { get; set; }
        
        public void Vote(Guid userId, bool isDownVote = false)
        {
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
            }
            else
            {
                if (isDownVote == vote.DownVote)
                    Votes.Remove(vote);
                else
                    vote.DownVote = isDownVote;
            }

            int upVote = Votes.Count(v => !v.DownVote);
            int downVote = Votes.Count(v => v.DownVote);
            Upvote = upVote -  downVote;
        }
    }
}
