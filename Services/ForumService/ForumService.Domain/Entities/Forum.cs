using System.ComponentModel.DataAnnotations;

namespace ForumService.ForumService.Domain.Entities
{
    public class Forum
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string ShortName { get; set; }
        public Guid CreatorId { get; set; }
        public string Description { get; set; }
        public string ForumBanner { get; set; }
        public string ForumImage { get; set; }
        public DateTime CreatedAt { get; set; }
        
        public Forum() { }
    }
}
