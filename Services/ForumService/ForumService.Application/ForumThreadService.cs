using ForumService.ForumService.Application.DTOs;
using ForumService.ForumService.Application.Interfaces.UnitOfWork;
using ForumService.ForumService.Domain.Entities;
using ForumService.ForumService.Infrastructure.Repositories;
using ForumService.ForumService.Application.Interfaces.Services;

namespace ForumService.ForumService.Application
{

  public class ForumThreadService : IForumThreadService
  {
    private readonly IUnitOfWork _unitOfWork;

    public ForumThreadService(IUnitOfWork unitOfWork)
    {
      _unitOfWork = unitOfWork;
    }

    public async Task<IEnumerable<ForumThreadDto>> GetThreadsByForumId(Guid forumId)
    {
      var threads = await _unitOfWork.ForumThreads.GetThreadsByForumIdAsync(forumId);

      var forumThreads = threads
              .Select(t => new ForumThreadDto
              {
                Id = t.ForumId,
                ForumId = t.ForumId,
                CreatorId = t.CreatorId,
                Title = t.Title,
                IsPinned = t.IsPinned,
                Tags = t.Tags,
                Images = t.Images,
                Content = t.Content,
                Slug = t.Slug,
                Upvote = t.Upvote,
                CreatedAt = t.CreatedAt,
                LastUpdatedAt = t.LastUpdatedAt
              })
              .ToList();

      return forumThreads;
    }

    public async Task<IEnumerable<CommentDto>> GetCommentsByThreadId(Guid threadId)
    {
      if (threadId == Guid.Empty)
      {
        throw new ArgumentNullException(nameof(threadId));
      }

      var comments = await _unitOfWork.ForumThreads.GetCommentByThreadIdAsync(threadId);

      return comments.Select(c => new CommentDto
      {
        Id = c.Id,
        ThreadId = c.ThreadId,
        OwnerId = c.OwnerId,
        Content = c.Content,
        Upvote = c.Upvote,
        CreatedAt = c.CreatedAt,
        ParentId = c.ParentId,
        ChildrenComments = c.ChildrenComments,
        UpdatedAt = c.UpdatedAt,
        Deleted = c.Deleted
      })
      .ToList();
    }

    public async Task InsertComment(CommentDto comment)
    {
      if (comment == null)
      {
        throw new ArgumentNullException(nameof(comment));
      }

      if (comment.ThreadId == Guid.Empty)
      {
        throw new ArgumentException("Thread Id cannot be empty", nameof(comment));
      }

      Comment cmt = new Comment
      {
        Id = Guid.NewGuid(),
        ThreadId = comment.ThreadId,
        OwnerId = comment.OwnerId,
        Content = comment.Content,
        Upvote = comment.Upvote,
        ParentId = comment.ParentId,
        ChildrenComments = comment.ChildrenComments,
        UpdatedAt = comment.UpdatedAt,
        CreatedAt = comment.CreatedAt,
        Deleted = comment.Deleted,
      };

      await _unitOfWork.ForumThreads.InsertCommentAsync(cmt);
      await _unitOfWork.CommitAsync();
    }
  }
}
