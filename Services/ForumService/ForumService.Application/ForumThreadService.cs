using ForumService.ForumService.Application.DTOs;
using ForumService.ForumService.Application.Enums;
using ForumService.ForumService.Application.Interfaces.UnitOfWork;
using ForumService.ForumService.Domain.Entities;
using ForumService.ForumService.Infrastructure.Repositories;
using ForumService.ForumService.Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace ForumService.ForumService.Application;

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
        PollItems = t.PollItems?.Select(p => new PollItemDto()
        {
          PollContent = p.PollContent,
          VoteCount = p.VoteCount,
        }).ToList()
      });
    }
    
    private List<CommentDto> MapCommentsToDto(IEnumerable<Comment> comments)
    {
      return comments.Select(c => new CommentDto
      {
        Id = c.Id,
        ThreadId = c.ThreadId,
        OwnerId = c.OwnerId,
        OwnerName = c.OwnerName,
        Content = c.Content,
        Upvote = c.Upvote,
        CreatedAt = c.CreatedAt,
        ParentId = c.ParentId,
        ChildrenComments = c.ChildrenComments != null
          ? MapCommentsToDto(c.ChildrenComments)
          : new List<CommentDto>(),
        UpdatedAt = c.UpdatedAt,
        Deleted = c.Deleted
      }).ToList();
    }
    

    public async Task<IEnumerable<CommentDto>> GetCommentsByThreadId(Guid threadId)
    {
      if (threadId == Guid.Empty)
      {
        throw new ArgumentNullException(nameof(threadId));
      }

      var comments = await _unitOfWork.ForumThreads.GetCommentByThreadIdAsync(threadId);

      return MapCommentsToDto(comments);
    }

    public async Task InsertComment(CommentDto comment, Guid userId, string ownerName)
    {
      if (comment.ThreadId == Guid.Empty)
      {
        throw new ArgumentException("Thread Id cannot be empty", nameof(comment));
      }

      var cmt = new Comment
      {
        Id = Guid.NewGuid(),
        ThreadId = comment.ThreadId,
        OwnerId = userId,
        OwnerName = ownerName,
        Content = comment.Content,
        Upvote = comment.Upvote,
        ParentId = comment.ParentId,
        ChildrenComments = new List<Comment>(),
        UpdatedAt = comment.UpdatedAt,
        CreatedAt = DateTime.UtcNow,
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

    public async Task CreateForumThread(ForumThreadDto forumThread, Guid userId)
    {
      Guid forumThreadId = Guid.NewGuid();
      ForumThread newThread = new()
      {
        Id = forumThreadId,
        Content = forumThread.Content,
        CreatedAt = DateTime.UtcNow,
        CreatorId = userId,
        ForumId = forumThread.ForumId,
        IsPinned = forumThread.IsPinned,
        LastUpdatedAt = DateTime.UtcNow,
        Images = forumThread.Images,
        PollItems = forumThread.PollItems?.Select(p => new Poll()
        {
          ThreadId = forumThreadId,
          PollContent = p.PollContent,
          VoteCount = p.VoteCount,
        }).ToList(),
        Title = forumThread.Title,
        Slug = forumThread.Slug,
      };
      
      await _unitOfWork.ForumThreads.InsertThreadAsync(newThread);
      await _unitOfWork.CommitAsync();
    }

    public async Task<ForumThreadDto?> GetThreadById(Guid threadId)
    {
      var thread = await _unitOfWork.ForumThreads.GetThreadByIdAsync(threadId);
      if (thread == null)
        return null;
      
      return new ForumThreadDto()
      {
        Id = thread.Id,
        CreatedAt = thread.CreatedAt,
        Content = thread.Content,
        CreatorId = thread.CreatorId,
        ForumId = thread.ForumId,
        Images = thread.Images,
        IsPinned = thread.IsPinned,
        LastUpdatedAt = thread.LastUpdatedAt,
        Slug = thread.Slug,
        Tags = thread.Tags,
        Upvote = thread.Upvote,
        Title = thread.Title,
        PollItems = thread.PollItems?.Select(p => new PollItemDto()
        {
          PollContent = p.PollContent,
          VoteCount = p.VoteCount,
        }).ToList(),
      };
    }
}

