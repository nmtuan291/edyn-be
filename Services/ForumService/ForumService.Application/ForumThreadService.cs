using AutoMapper;
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
    private readonly ICommentNotificationSender _commentNotificationSender;
    private readonly IMapper _mapper;

    public ForumThreadService(IUnitOfWork unitOfWork, ICommentNotificationSender commentNotificationSender, IMapper mapper)
    {
      _unitOfWork = unitOfWork;
      _commentNotificationSender = commentNotificationSender;
      _mapper =  mapper;
    }

    public async Task<List<ForumThreadDto>> GetThreadsByForumId(Guid forumId, string? userId, int pageNumber, int pageSize, 
      SortBy sortBy = SortBy.Hot, SortDate sortDate = SortDate.All)
    {
      var threads = await _unitOfWork.ThreadRepo.GetThreadsByForumIdAsync(forumId, sortBy, 
        sortDate, pageNumber, pageSize);

      var votedThreads = string.IsNullOrEmpty(userId)
        ? new Dictionary<Guid, bool>()
        : await _unitOfWork.VoteRepo.GetVotedThreadsAsync(Guid.Parse(userId), forumId);;

      return _mapper.Map<List<ForumThreadDto>>(threads)
        .Select(dto =>
        {
          if (!string.IsNullOrEmpty(userId))
          {
            dto.Vote = votedThreads.TryGetValue(dto.Id!.Value, out var v) ? (v ? "down" : "up") : "none";
          }
          else
          {
            dto.Vote = "none";
          }
          return dto;
        })
        .ToList();
    }

    public async Task<List<CommentDto>> GetCommentsByThreadId(Guid threadId)
    {
      if (threadId == Guid.Empty)
        throw new ArgumentNullException(nameof(threadId));

      var comments = await _unitOfWork.CommentRepo.GetCommentByThreadIdAsync(threadId);

      return _mapper.Map<List<CommentDto>>(comments);
    }

    public async Task InsertComment(CommentDto comment, Guid userId, string ownerName)
    {
      if (comment.ThreadId == Guid.Empty)
        throw new ArgumentException("Thread Id cannot be empty", nameof(comment));

      var cmt = new Comment(comment.ThreadId, userId, ownerName, comment.Content, comment.ParentId ?? Guid.Empty);

      await _unitOfWork.CommentRepo.InsertCommentAsync(cmt);
      await _unitOfWork.CommitAsync();
      
      var parentComment = await _unitOfWork.CommentRepo.GetParentCommentAsync(cmt.ParentId ?? Guid.Empty);
      await _commentNotificationSender.SendNotification(parentComment?.OwnerId.ToString() ?? "", 
        parentComment?.OwnerName ?? "",parentComment?.Content ?? "", parentComment?.ThreadId.ToString() ?? "");
    }

    public async Task DeleteComment(Guid commentId)
    {
      await _unitOfWork.CommentRepo.DeleteCommentById(commentId);
      await _unitOfWork.CommitAsync();
    }

    public async Task DeleteThread(Guid threadId)
    {
      await _unitOfWork.ThreadRepo.DeleteThreadByIdAsync(threadId);
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
      
      await _unitOfWork.ThreadRepo.InsertThreadAsync(newThread);
      await _unitOfWork.CommitAsync();
    }

    public async Task<ForumThreadDto?> GetThreadById(Guid threadId)
    {
      var thread = await _unitOfWork.ThreadRepo.GetThreadByIdAsync(threadId);
      if (thread == null)
        return null;
      
      return _mapper.Map<ForumThreadDto>(thread);
    }
    
    /*public async Task<List<ForumThreadDto>> GetThreadsByJoinForums(Guid userId, int page = 0, int pageSize = 10)
    {
      var joinedForums =  await _unitOfWork.Forums.GetJoinedForumsByUserIdAsync(userId);
      List<ForumThreadDto> threads = new List<ForumThreadDto>();

      foreach (var forum in joinedForums)
      {
        var thread = await _unitOfWork.Forums.GetForumByNameAsync(forum.Forum.Name);
        // TODO
      }
    }*/
    
    public async Task<ForumThreadDto?> UpdateThreadVote(Guid threadId, Guid userId, bool isDownVote = false)
    {
      var thread = await _unitOfWork.ThreadRepo.GetThreadByIdAsync(threadId);
      if (thread == null)
        return null;
      
      thread.Vote(userId, isDownVote);
      await _unitOfWork.CommitAsync();
      
      return _mapper.Map<ForumThreadDto>(thread);
    }
}

