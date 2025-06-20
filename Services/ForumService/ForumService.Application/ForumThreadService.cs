using ForumService.ForumService.Application.DTOs;
using ForumService.ForumService.Application.Enums;
using ForumService.ForumService.Application.Interfaces.UnitOfWork;
using ForumService.ForumService.Domain.Entities;
using ForumService.ForumService.Infrastructure.Repositories;
using ForumService.ForumService.Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace ForumService.ForumService.Application
{

  public class ForumThreadService : IForumThreadService
  {
    private readonly IUnitOfWork _unitOfWork;

    public ForumThreadService(IUnitOfWork unitOfWork)
    {
      _unitOfWork = unitOfWork;
    }

    public async Task<IEnumerable<ForumThreadDto>> GetThreadsByForumId(Guid forumId, int pageNumber, int pageSize, 
      SortBy sortBy = SortBy.Hot, SortDate sortDate = SortDate.All)
    {
      var threads = await _unitOfWork.ForumThreads.GetThreadsByForumIdAsync(forumId, sortBy, 
        sortDate, pageNumber, pageSize);
      
      return threads.Select(t => new ForumThreadDto()
      {
        Id = t.Id,
        CreatedAt = t.CreatedAt,
        Content = t.Content,
        CreatorId = t.CreatorId,
        ForumId = t.ForumId,
        Images = t.Images,
        IsPinned = t.IsPinned,
        LastUpdatedAt = t.LastUpdatedAt,
        Slug = t.Slug,
        Tags = t.Tags,
        Upvote = t.Upvote,
        Title = t.Title,
      });
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

    public async Task DeleteComment(Guid commentId)
    {
      await _unitOfWork.ForumThreads.DeleteCommentById(commentId);
      await _unitOfWork.CommitAsync();
    }

    public async Task DeleteThread(Guid threadId)
    {
      await _unitOfWork.ForumThreads.DeleteThreadByIdAsync(threadId);
      await _unitOfWork.CommitAsync();
    }
}
}
