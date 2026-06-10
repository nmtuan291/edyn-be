using AutoMapper;
using ForumService.ForumService.Application.DTOs;
using ForumService.ForumService.Application.Enums;
using ForumService.ForumService.Application.Interfaces.Services;
using ForumService.ForumService.Application.Interfaces.UnitOfWork;
using MediatR;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Retry;
using UserService.Grpc;

namespace ForumService.ForumService.Application.Features.Forums.Queries.GetForumMembers;

public sealed class GetForumMembersQueryHandler : IRequestHandler<GetForumMembersQuery, List<ForumMemberDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<GetForumMembersQueryHandler> _logger;
    private readonly IPermissionService _permissionService;
    private readonly UserProfileService.UserProfileServiceClient _userProfileService;
    private readonly AsyncRetryPolicy _retryPolicy;

    public GetForumMembersQueryHandler(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<GetForumMembersQueryHandler> logger,
        IPermissionService permissionService,
        UserProfileService.UserProfileServiceClient userProfileService)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
        _permissionService = permissionService;
        _userProfileService = userProfileService;
        _retryPolicy = Policy
            .Handle<Exception>()
            .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));
    }

    public async Task<List<ForumMemberDto>> Handle(GetForumMembersQuery request, CancellationToken cancellationToken)
    {
        var forumUsers = await _unitOfWork.ForumRepo.GetForumUsersAsync(request.ForumId, cancellationToken);
        if (forumUsers.Count == 0)
            return [];

        var userIds = forumUsers.Select(f => f.UserId.ToString()).Distinct().ToList();

        Dictionary<Guid, UserDto> byId = new();
        try
        {
            await _retryPolicy.ExecuteAsync(async () =>
            {
                var profileRequest = new ProfileRequest();
                profileRequest.Id.AddRange(userIds);
                var response = await _userProfileService.GetUserProfileAsync(
                    profileRequest,
                    cancellationToken: cancellationToken);

                var users = _mapper.Map<List<UserDto>>(response.Profiles);
                byId = users.ToDictionary(u => u.Id);
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch forum member profiles from UserService");
        }

        var members = new List<ForumMemberDto>(forumUsers.Count);
        foreach (var forumUser in forumUsers)
        {
            if (!byId.TryGetValue(forumUser.UserId, out var profile))
            {
                profile = new UserDto
                {
                    Id = forumUser.UserId,
                    Username = forumUser.UserId.ToString(),
                    Avatar = string.Empty,
                    Bio = string.Empty,
                    BirthDay = default,
                    Gender = null,
                };
            }

            var effective = await _permissionService.GetEffectivePermissionsAsync(
                request.ForumId,
                forumUser.UserId,
                cancellationToken);

            members.Add(new ForumMemberDto
            {
                Id = profile.Id,
                Username = profile.Username,
                Avatar = profile.Avatar,
                Bio = profile.Bio,
                BirthDay = profile.BirthDay,
                Gender = profile.Gender,
                Role = forumUser.Role,
                EffectivePermissions = effective,
                PermissionOverrides = forumUser.PermissionOverrides.HasValue
                    ? (ForumPermissionType)forumUser.PermissionOverrides.Value
                    : null,
            });
        }

        return members;
    }
}
