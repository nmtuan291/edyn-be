using ForumService.ForumService.Domain.Entities;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Grpc.Net.Client.Balancer;

namespace ForumService.ForumService.Application.DTOs
{
    public class CommentDto
    {
        public Guid? Id { get; set; }
        public required Guid ThreadId { get; set; }
        public Guid? OwnerId { get; set; }
        public string? OwnerName { get; set; }
        public required string Content { get; set; }
        public required int Upvote { get; set; }
        public Guid? ParentId { get; set; }
        public ICollection<CommentDto>? ChildrenComments { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public DateTime? CreatedAt { get; set; }
        public required bool Deleted { get; set; }
    }
}
