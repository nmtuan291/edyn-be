using AutoMapper;
using ForumService.ForumService.Application.DTOs;
using ForumService.ForumService.Application.Enums;
using ForumService.ForumService.Application.Interfaces.Services;
using ForumService.ForumService.Application.Interfaces.UnitOfWork;
using ForumService.ForumService.Domain.Entities;
using MediatR;
using Polly;
using Polly.Retry;
using UserService.Grpc;

namespace ForumService.ForumService.Application.Features.Threads.Commands.CreateThread;

public sealed class CreateThreadCommandHandler : IRequestHandler<CreateThreadCommand, CreateThreadResult>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPermissionService _permissionService;
    private readonly IMapper _mapper;
    private readonly UserProfileService.UserProfileServiceClient _userProfileService;
    private readonly AsyncRetryPolicy _retryPolicy;

    public CreateThreadCommandHandler(
        IUnitOfWork unitOfWork,
        IPermissionService permissionService,
        IMapper mapper,
        UserProfileService.UserProfileServiceClient userProfileService)
    {
        _unitOfWork = unitOfWork;
        _permissionService = permissionService;
        _mapper = mapper;
        _userProfileService = userProfileService;
        _retryPolicy = Policy
            .Handle<Exception>()
            .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));
    }

    public async Task<CreateThreadResult> Handle(CreateThreadCommand request, CancellationToken cancellationToken)
    {
        var canCreate = await _permissionService.HasPermissionAsync(
            request.Thread.ForumId,
            request.UserId,
            ForumPermissionType.CreateThread,
            cancellationToken);

        if (!canCreate)
            return new CreateThreadResult(null, Forbidden: true);

        var threadId = Guid.NewGuid();
        var newThread = new ForumThread
        {
            Id = threadId,
            Content = request.Thread.Content,
            CreatedAt = DateTime.UtcNow,
            CreatorId = request.UserId,
            ForumId = request.Thread.ForumId,
            IsPinned = request.Thread.IsPinned,
            LastUpdatedAt = DateTime.UtcNow,
            Images = request.Thread.Images,
            PollItems = request.Thread.PollItems?.Select(p => new Poll
            {
                ThreadId = threadId,
                PollContent = p.PollContent,
                VoteCount = p.VoteCount,
            }).ToList(),
            Title = request.Thread.Title,
            Slug = request.Thread.Slug,
            Tags = BuildTagsFromDto(request.Thread.Tags),
        };

        try
        {
            await _retryPolicy.ExecuteAsync(async () =>
            {
                var profileRequest = new ProfileRequest();
                profileRequest.Id.Add(request.UserId.ToString());
                var response = await _userProfileService.GetUserProfileAsync(profileRequest, cancellationToken: cancellationToken);
                var profile = response.Profiles.FirstOrDefault();
                if (profile != null)
                {
                    newThread.CreatorName = profile.Username;
                    newThread.CreatorAvatar = profile.Avatar;
                }
            });
        }
        catch
        {
            // Match existing behavior: profile lookup failure should not block thread creation.
        }

        await _unitOfWork.ThreadRepo.InsertThreadAsync(newThread);
        await _unitOfWork.CommitAsync();

        var saved = await _unitOfWork.ThreadRepo.GetThreadByIdAsync(threadId, cancellationToken: cancellationToken);
        return new CreateThreadResult(_mapper.Map<ForumThreadDto>(saved!));
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
}
