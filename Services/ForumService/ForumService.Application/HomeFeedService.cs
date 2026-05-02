using AutoMapper;
using ForumService.ForumService.Application.DTOs;
using ForumService.ForumService.Application.Enums;
using ForumService.ForumService.Application.Interfaces.Services;
using ForumService.ForumService.Application.Interfaces.UnitOfWork;
using ForumService.ForumService.Domain.Entities;

namespace ForumService.ForumService.Application;

public class HomeFeedService : IHomeFeedService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    private const int CandidateMultiplier = 5;
    private const int MaxPerForum = 5;
    private const double TagAffinityBoost = 1.2;
    private static readonly TimeSpan FeedCutoff = TimeSpan.FromDays(7);

    public HomeFeedService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<List<ForumThreadDto>> GetHomeFeed(string? userId, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        List<Guid>? forumIds = null;
        Guid? parsedUserId = null;

        if (!string.IsNullOrEmpty(userId))
        {
            parsedUserId = Guid.Parse(userId);
            forumIds = await _unitOfWork.ForumRepo.GetJoinedForumIdsAsync(parsedUserId.Value, cancellationToken);
            if (forumIds.Count == 0)
                forumIds = null;
        }

        var candidateCount = pageSize * CandidateMultiplier;
        var cutoff = DateTime.UtcNow - FeedCutoff;

        var candidates = await _unitOfWork.ThreadRepo.GetHomeFeedCandidatesAsync(forumIds, candidateCount, cutoff, cancellationToken);

        Dictionary<string, double>? tagAffinity = null;
        if (parsedUserId.HasValue)
            tagAffinity = await _unitOfWork.VoteRepo.GetTagAffinityAsync(parsedUserId.Value);

        var scored = candidates
            .Select(t => new { Thread = t, Score = ComputeBoostedScore(t, tagAffinity) })
            .OrderByDescending(x => x.Score)
            .ToList();

        var diversified = ApplyDiversityCap(scored.Select(x => x.Thread).ToList(), MaxPerForum);

        var paged = diversified
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        var forumIdsInResult = paged.Select(t => t.ForumId).Distinct();
        var forumInfo = await _unitOfWork.ForumRepo.GetForumInfoByIdsAsync(forumIdsInResult, cancellationToken);

        var votedByForum = new Dictionary<Guid, Dictionary<Guid, bool>>();
        if (parsedUserId.HasValue)
        {
            foreach (var fId in paged.Select(t => t.ForumId).Distinct())
            {
                votedByForum[fId] = await _unitOfWork.VoteRepo.GetVotedThreadsAsync(parsedUserId.Value, fId);
            }
        }

        return paged.Select(thread =>
        {
            var dto = _mapper.Map<ForumThreadDto>(thread);

            if (forumInfo.TryGetValue(thread.ForumId, out var info))
            {
                dto.ForumName = info.Name;
                dto.ForumImage = info.Image;
            }

            dto.Vote = VoteStatus.NoVote;
            if (parsedUserId.HasValue &&
                votedByForum.TryGetValue(thread.ForumId, out var votes) &&
                dto.Id.HasValue &&
                votes.TryGetValue(dto.Id.Value, out var isDownVote))
            {
                dto.Vote = isDownVote ? VoteStatus.DownVote : VoteStatus.UpVote;
            }

            return dto;
        }).ToList();
    }

    private static double ComputeBoostedScore(ForumThread thread, Dictionary<string, double>? tagAffinity)
    {
        var baseScore = ComputeHotScore(thread.Upvote, thread.CreatedAt);

        if (tagAffinity is not { Count: > 0 } || thread.Tags is not { Count: > 0 })
            return baseScore;

        var hasMatchingTag = thread.Tags.Any(t => tagAffinity.ContainsKey(t.Name));
        return hasMatchingTag ? baseScore * TagAffinityBoost : baseScore;
    }

    private static double ComputeHotScore(int upvotes, DateTime createdAt)
    {
        var ageInHours = (DateTime.UtcNow - createdAt).TotalHours;
        return upvotes / Math.Pow(ageInHours + 2, 1.5);
    }

    private static List<ForumThread> ApplyDiversityCap(List<ForumThread> items, int maxPerGroup)
    {
        var result = new List<ForumThread>();
        var countByForum = new Dictionary<Guid, int>();

        foreach (var item in items)
        {
            countByForum.TryGetValue(item.ForumId, out var count);
            if (count >= maxPerGroup)
                continue;

            result.Add(item);
            countByForum[item.ForumId] = count + 1;
        }

        return result;
    }
}
