using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

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
        public Comment() { }

        public Comment(Guid threadId, Guid ownerId, string ownerName,string content, Guid parentId)
        {
            if (threadId == Guid.Empty)
                throw new ArgumentException("ThreadId cannot be empty");
            if (ownerId == Guid.Empty)
                throw new ArgumentException("OwnerId cannot be empty");
            /*if (string.IsNullOrEmpty(ownerName))
                throw new ArgumentException("OwnerName cannot be empty");*/
            
            Id = Guid.NewGuid();
            ThreadId = threadId;
            OwnerId = ownerId;
            OwnerName = ownerName;
            Content = content;
            Upvote = 0;
            ParentId = parentId;
            ChildrenComments = new List<Comment>();
            UpdatedAt = DateTime.UtcNow;
            CreatedAt = DateTime.UtcNow;
            Deleted = false;
        }
    }
}
