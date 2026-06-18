using AutoMapper;
using ForumService.ForumService.Application.DTOs;
using ForumService.ForumService.Application.Enums;
using ForumService.ForumService.Application.Interfaces.Repositories;
using ForumService.ForumService.Domain.Entities;
using MediatR;

namespace ForumService.ForumService.Application.Features.Feeds.Queries.GetHomeFeed;

public sealed class GetHomeFeedQueryHandler : IRequestHandler<GetHomeFeedQuery, List<ForumThreadDto>>
{
    private readonly IForumQueryRepository _forumRepository;
    private readonly IThreadQueryRepository _threadRepository;
    private readonly IVoteQueryRepository _voteRepository;
    private readonly IMapper _mapper;

    private const int CandidateMultiplier = 5;
    private const int MaxPerForum = 5;
    private const double TagAffinityBoost = 1.2;
    private static readonly TimeSpan FeedCutoff = TimeSpan.FromDays(365);

    public GetHomeFeedQueryHandler(
        IForumQueryRepository forumRepository,
        IThreadQueryRepository threadRepository,
        IVoteQueryRepository voteRepository,
        IMapper mapper)
    {
        _forumRepository = forumRepository;
        _threadRepository = threadRepository;
        _voteRepository = voteRepository;
        _mapper = mapper;
    }

    public async Task<List<ForumThreadDto>> Handle(
        GetHomeFeedQuery request,
        CancellationToken cancellationToken)
    {
        List<Guid>? forumIds = null;
        Guid? parsedUserId = null;

        if (!string.IsNullOrEmpty(request.UserId))
        {
            parsedUserId = Guid.Parse(request.UserId);
            forumIds = await _forumRepository.GetJoinedForumIdsAsync(parsedUserId.Value, cancellationToken);
            if (forumIds.Count == 0)
                forumIds = null;
        }

        var candidateCount = request.PageSize * CandidateMultiplier;

        // "Top" honours an explicit time window; other modes use the default feed cutoff.
        var cutoff = request.Sort == SortBy.Top
            ? GetDateCutoff(request.Date)
            : DateTime.UtcNow - FeedCutoff;

        var candidates = await _threadRepository.GetHomeFeedCandidatesAsync(
            forumIds,
            candidateCount,
            cutoff,
            request.Sort,
            cancellationToken);

        List<ForumThread> ranked;
        switch (request.Sort)
        {
            case SortBy.Latest:
                // Candidates are already ordered by CreatedAt descending at the DB level.
                ranked = candidates;
                break;

            case SortBy.Top:
                ranked = candidates
                    .OrderByDescending(t => t.Upvote)
                    .ThenByDescending(t => t.CreatedAt)
                    .ToList();
                break;

            default: // Hot
                Dictionary<string, double>? tagAffinity = null;
                if (parsedUserId.HasValue)
                    tagAffinity = await _voteRepository.GetTagAffinityAsync(parsedUserId.Value);

                ranked = candidates
                    .Select(t => new { Thread = t, Score = ComputeBoostedScore(t, tagAffinity) })
                    .OrderByDescending(x => x.Score)
                    .Select(x => x.Thread)
                    .ToList();
                break;
        }

        var diversified = ApplyDiversityCap(ranked, MaxPerForum);

        var paged = diversified
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToList();

        var forumIdsInResult = paged.Select(t => t.ForumId).Distinct();
        var forumInfo = await _forumRepository.GetForumInfoByIdsAsync(forumIdsInResult, cancellationToken);

        var votedByForum = new Dictionary<Guid, Dictionary<Guid, bool>>();
        if (parsedUserId.HasValue)
        {
            foreach (var forumId in paged.Select(t => t.ForumId).Distinct())
            {
                votedByForum[forumId] = await _voteRepository.GetVotedThreadsAsync(parsedUserId.Value, forumId);
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

    private static DateTime GetDateCutoff(SortDate date) => date switch
    {
        SortDate.Day => DateTime.UtcNow.AddDays(-1),
        SortDate.Month => DateTime.UtcNow.AddDays(-30),
        SortDate.Year => DateTime.UtcNow.AddDays(-365),
        _ => DateTime.MinValue.ToUniversalTime(),
    };

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
