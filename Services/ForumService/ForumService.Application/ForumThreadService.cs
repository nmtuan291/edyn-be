using AutoMapper;
using ForumService.ForumService.Application.DTOs;
using ForumService.ForumService.Application.Enums;
using ForumService.ForumService.Application.Interfaces.UnitOfWork;
using ForumService.ForumService.Application.Requests;
using ForumService.ForumService.Domain.Entities;
using ForumService.ForumService.Domain.ValueObjects;
using ForumService.ForumService.Application.Interfaces.Services;
using UserService.Grpc;
using Polly;
using Polly.Retry;

namespace ForumService.ForumService.Application;

public class ForumThreadService : IForumThreadService
{
  private readonly IUnitOfWork _unitOfWork;
  private readonly ICommentNotificationSender _commentNotificationSender;
  private readonly IMapper _mapper;
  private readonly UserProfileService.UserProfileServiceClient _userProfileService;
  private readonly AsyncRetryPolicy _retryPolicy;

  public ForumThreadService(
    IUnitOfWork unitOfWork, 
    ICommentNotificationSender commentNotificationSender, 
    IMapper mapper,
    UserProfileService.UserProfileServiceClient userProfileService)
  {
    _unitOfWork = unitOfWork;
    _commentNotificationSender = commentNotificationSender;
    _mapper =  mapper;
    _userProfileService = userProfileService;
    _retryPolicy = Policy
        .Handle<Exception>()
        .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));
  }

  public async Task<List<ForumThreadDto>> GetThreadsByForumId(ForumThreadListQuery query, CancellationToken cancellationToken = default)
  {
    var page = new ForumThreadPageQuery(
      query.ForumId, query.SortBy, query.SortDate, query.PageNumber, query.PageSize);
    var threads = await _unitOfWork.ThreadRepo.GetThreadsByForumIdAsync(page, cancellationToken);

    var votedThreads = string.IsNullOrEmpty(query.UserId)
      ? new Dictionary<Guid, bool>()
      : await _unitOfWork.VoteRepo.GetVotedThreadsAsync(Guid.Parse(query.UserId), query.ForumId);

    var dtos = _mapper.Map<List<ForumThreadDto>>(threads)
      .Select(dto =>
      {
        if (!string.IsNullOrEmpty(query.UserId))
        {
          if (votedThreads.TryGetValue(dto.Id!.Value, out var v))
          {
            if (v)
              dto.Vote = VoteStatus.DownVote;
            else
              dto.Vote = VoteStatus.UpVote;
          }
          else
            dto.Vote = VoteStatus.NoVote;
        }
        else
        {
          dto.Vote = VoteStatus.NoVote;
        }
        return dto;
      })
      .ToList();

    return dtos;
  }

  public async Task<List<CommentDto>> GetCommentsByThreadId(Guid threadId, string? userId, CancellationToken cancellationToken = default)
  {
    if (threadId == Guid.Empty)
      throw new ArgumentNullException(nameof(threadId));

    var comments = await _unitOfWork.CommentRepo.GetCommentByThreadIdAsync(threadId, cancellationToken);
    var votedComments = string.IsNullOrEmpty(userId) 
      ? new Dictionary<Guid, bool>()
      : await _unitOfWork.VoteRepo.GetVotedCommentsAsync(Guid.Parse(userId), threadId);

    return _mapper.Map<List<CommentDto>>(comments)
      .Select(dto =>
      {
        if (!string.IsNullOrEmpty(userId))
        {
          if (votedComments.TryGetValue(dto.Id!.Value, out var v))
          {
            if (v)
              dto.Vote = VoteStatus.DownVote;
            else
              dto.Vote = VoteStatus.UpVote;
          }
          else
            dto.Vote = VoteStatus.NoVote;
        }
        else
        {
          dto.Vote = VoteStatus.NoVote;
        }
        return dto;
      }).ToList();
  }

  public async Task InsertComment(CommentDto comment, Guid userId, string ownerName)
  {
    if (comment.ThreadId == Guid.Empty)
      throw new ArgumentException("Thread Id cannot be empty", nameof(comment));

    if (comment.ParentId is { } parentId && parentId != Guid.Empty)
    {
      var parent = await _unitOfWork.CommentRepo.GetCommentByIdAsync(parentId);
      if (parent == null || parent.ThreadId != comment.ThreadId)
        throw new ArgumentException("Parent comment does not exist or is not on this thread.");
    }

    var cmt = new Comment(new CommentCreation(
      comment.ThreadId, userId, ownerName, comment.Content, comment.ParentId));

    await _unitOfWork.CommentRepo.InsertCommentAsync(cmt);

    var parentComment = cmt.ParentId is { } pid && pid != Guid.Empty
      ? await _unitOfWork.CommentRepo.GetParentCommentAsync(pid)
      : null;
      
    await _commentNotificationSender.SendNotification(new CommentNotificationMessage(
      parentComment?.OwnerId.ToString() ?? "",
      ownerName,
      parentComment?.Content ?? "",
      parentComment?.ThreadId.ToString() ?? ""));
      
    await _unitOfWork.CommitAsync();
  }

  public async Task DeleteComment(Guid commentId, Guid userId)
  {
    var comment = await _unitOfWork.CommentRepo.GetCommentByIdAsync(commentId);
    if (comment == null)
      throw new KeyNotFoundException("Comment not found.");
    if (comment.OwnerId != userId)
      throw new UnauthorizedAccessException("You can only delete your own comments.");

    await _unitOfWork.CommentRepo.DeleteCommentById(commentId);
    await _unitOfWork.CommitAsync();
  }

  public async Task DeleteThread(Guid threadId, Guid userId)
  {
    var thread = await _unitOfWork.ThreadRepo.GetThreadByIdAsync(threadId);
    if (thread == null)
      throw new KeyNotFoundException("Thread not found.");
    if (thread.CreatorId != userId)
      throw new UnauthorizedAccessException("You can only delete your own threads.");

    await _unitOfWork.ThreadRepo.DeleteThreadByIdAsync(threadId);
    await _unitOfWork.CommitAsync();
  }

  public async Task<ForumThreadDto> CreateForumThread(ForumThreadDto forumThread, Guid userId)
  {
    Guid forumThreadId = Guid.NewGuid();

    var tags = BuildTagsFromDto(forumThread.Tags);

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
      Tags = tags,
    };
    
    try
    {
      await _retryPolicy.ExecuteAsync(async () =>
      {
        var request = new ProfileRequest();
        request.Id.Add(userId.ToString());
        var response = await _userProfileService.GetUserProfileAsync(request);
        var profile = response.Profiles.FirstOrDefault();
        if (profile != null)
        {
          newThread.CreatorName = profile.Username;
          newThread.CreatorAvatar = profile.Avatar;
        }
      });
    }
    catch (Exception ex)
    {
      // Log error but continue
    }
    
    await _unitOfWork.ThreadRepo.InsertThreadAsync(newThread);
    await _unitOfWork.CommitAsync();

    var saved = await _unitOfWork.ThreadRepo.GetThreadByIdAsync(forumThreadId);
    return _mapper.Map<ForumThreadDto>(saved!);
  }

  private static List<Tag> BuildTagsFromDto(ICollection<Tag>? dtoTags)
  {
    if (dtoTags == null || dtoTags.Count == 0)
      return new List<Tag>();

    return dtoTags
      .Where(t => t != null && !string.IsNullOrWhiteSpace(t.Name))
      .Select(t => new Tag
      {
          Id = 0,
          Name = t.Name.Trim(),
          Color = string.IsNullOrWhiteSpace(t.Color) ? "#808080" : t.Color.Trim(),
      })
      .GroupBy(t => t.Name, StringComparer.OrdinalIgnoreCase)
      .Select(g => g.First())
      .ToList();
  }

  public async Task<ForumThreadDto?> GetThreadById(Guid threadId, string? userId, CancellationToken cancellationToken = default)
  {
    var parsedUserId =  Guid.TryParse(userId, out var userIdGuid) ? userIdGuid : Guid.Empty;
    
    var thread = await _unitOfWork.ThreadRepo.GetThreadByIdAsync(threadId, parsedUserId, cancellationToken);
    if (thread == null)
      return null;
    
    var dto = _mapper.Map<ForumThreadDto>(thread);
    if (!string.IsNullOrEmpty(userId) && Guid.TryParse(userId, out var userGuid))
    {
        dto.UserPollVote = thread.PollVotes?.FirstOrDefault(v => v.UserId == userGuid)?.PollContent;
    }
    
    return dto;
  }
  
  public async Task<ForumThreadDto?> UpdateThreadVote(Guid threadId, Guid userId, bool isDownVote)
  {
    var thread = await _unitOfWork.ThreadRepo.GetThreadByIdAsync(threadId);
    if (thread == null)
      return null;
    
    bool isVoteExists = thread.Vote(userId, isDownVote);
    _unitOfWork.ThreadRepo.UpdateThread(thread);
    
    if (isVoteExists)
    {
      await _unitOfWork.VoteRepo.UpdateThreadVoteRedisAsync(
          new ThreadVoteRedisUpdate(userId, threadId, thread.ForumId, isDownVote));
      var tagNames = thread.Tags?.Select(t => t.Name).Where(n => !string.IsNullOrEmpty(n)).ToList();
      if (tagNames is { Count: > 0 })
      {
        if (!isDownVote)
          await _unitOfWork.VoteRepo.IncrementTagAffinityAsync(userId, tagNames);
        else
          await _unitOfWork.VoteRepo.DecrementTagAffinityAsync(userId, tagNames);
      }
    }
    else
      await _unitOfWork.VoteRepo.RemoveThreadVoteRedisAsync(userId, threadId, thread.ForumId);
    
    await _unitOfWork.CommitAsync();
    
    return _mapper.Map<ForumThreadDto>(thread);
  }
  
  public async Task<CommentDto?> UpdateCommentVote(Guid commentId, Guid userId, bool isDownVote)
  {
    var comment = await _unitOfWork.CommentRepo.GetCommentByIdAsync(commentId);
    if (comment == null)
      return null;
    
    bool isVoteExists = comment.Vote(userId, isDownVote);
    await _unitOfWork.CommentRepo.UpdateCommentAsync(comment);
    
    if (isVoteExists)
      await _unitOfWork.VoteRepo.UpdateCommentVoteRedisAsync(
          new CommentVoteRedisUpdate(userId, commentId, comment.ThreadId, isDownVote));
    else
      await _unitOfWork.VoteRepo.RemoveCommentVoteRedisAsync(userId, commentId, comment.ThreadId);
    
    await _unitOfWork.CommitAsync();
    
    return _mapper.Map<CommentDto>(comment);
  }

  public async Task<ForumThreadDto?> EditThread(Guid threadId, Guid userId, EditThreadRequest request)
  {
    var thread = await _unitOfWork.ThreadRepo.GetThreadByIdAsync(threadId);
    if (thread == null)
      return null;

    if (thread.CreatorId != userId)
      throw new UnauthorizedAccessException("You can only edit your own threads.");

    if (request.Title != null)
      thread.Title = request.Title;
    if (request.Content != null)
      thread.Content = request.Content;
    thread.LastUpdatedAt = DateTime.UtcNow;

    _unitOfWork.ThreadRepo.UpdateThread(thread);
    await _unitOfWork.CommitAsync();

    return _mapper.Map<ForumThreadDto>(thread);
  }

  public async Task<CommentDto?> EditComment(Guid commentId, Guid userId, EditCommentRequest request)
  {
    var comment = await _unitOfWork.CommentRepo.GetCommentByIdAsync(commentId);
    if (comment == null)
      return null;

    if (comment.OwnerId != userId)
      throw new UnauthorizedAccessException("You can only edit your own comments.");

    comment.Content = request.Content;
    comment.UpdatedAt = DateTime.UtcNow;

    await _unitOfWork.CommentRepo.UpdateCommentAsync(comment);
    await _unitOfWork.CommitAsync();

    return _mapper.Map<CommentDto>(comment);
  }

  public async Task<PagedResult<ForumThreadDto>> GetThreadsByForumIdPaged(ForumThreadListQuery query, CancellationToken cancellationToken = default)
  {
    var items = await GetThreadsByForumId(query, cancellationToken);
    var totalCount = await _unitOfWork.ThreadRepo.GetThreadCountByForumIdAsync(query.ForumId, cancellationToken);

    return new PagedResult<ForumThreadDto>
    {
      Items = items,
      Page = query.PageNumber,
      PageSize = query.PageSize,
      TotalCount = totalCount,
    };
  }

  public async Task<PagedResult<ForumThreadDto>> SearchThreads(string query, int page, int pageSize, CancellationToken cancellationToken = default)
  {
    var threads = await _unitOfWork.ThreadRepo.SearchThreadsAsync(query, page, pageSize, cancellationToken);
    var totalCount = await _unitOfWork.ThreadRepo.SearchThreadsCountAsync(query, cancellationToken);
    var dtos = _mapper.Map<List<ForumThreadDto>>(threads);

    return new PagedResult<ForumThreadDto>
    {
      Items = dtos,
      Page = page,
      PageSize = pageSize,
      TotalCount = totalCount,
    };
  }

  public async Task<ForumThreadDto?> VotePoll(Guid userId, Guid threadId, string pollContent)
  {
    var thread = await _unitOfWork.ThreadRepo.VotePollAsync(userId, threadId, pollContent);
    if (thread == null)
      return null;

    await _unitOfWork.CommitAsync();

    var dto = _mapper.Map<ForumThreadDto>(thread);
    dto.UserPollVote = thread.PollVotes?.FirstOrDefault(v => v.UserId == userId)?.PollContent;
    return dto;
  }

  public async Task<PagedResult<UserCommentDto>> GetUserComments(Guid userId, int page, int pageSize,
    string? currentUserId, CancellationToken cancellationToken = default)
  {
    return new PagedResult<UserCommentDto>()
    {
      Items = [],
      Page = page,
      PageSize = pageSize,
      TotalCount = 1
    };
  }
}
