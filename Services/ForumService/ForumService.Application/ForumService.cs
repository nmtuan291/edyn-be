using AutoMapper;
using ForumService.ForumService.Application.DTOs;
using ForumService.ForumService.Application.Enums;
using ForumService.ForumService.Application.Exceptions;
using ForumService.ForumService.Application.Requests;
using ForumService.ForumService.Application.Interfaces.Services;
using ForumService.ForumService.Application.Interfaces.UnitOfWork;
using ForumService.ForumService.Domain.Entities;
using Polly;
using Polly.Retry;
using UserService.Grpc;

namespace ForumService.ForumService.Application
{
    public class ForumService : IForumService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<IForumService> _logger;
        private readonly IPermissionService _permissionService;
        private readonly UserProfileService.UserProfileServiceClient _userProfileService;
        private readonly AsyncRetryPolicy _retryPolicy;

        public ForumService(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ILogger<IForumService> logger,
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

        public async Task<ForumDto?> GetForum(Guid userId, string forumName, CancellationToken cancellationToken = default)
        {
            Forum? forum = await _unitOfWork.ForumRepo.GetForumByNameAsync(userId, forumName, cancellationToken);
            if (forum == null)
                return null;

            return _mapper.Map<ForumDto>(forum);
        }

        public async Task<List<ForumDto>> GetForums(CancellationToken cancellationToken = default)
        {
            var forums = await _unitOfWork.ForumRepo.GetForumsAsync(cancellationToken);
            return _mapper.Map<List<ForumDto>>(forums);
        }

        public async Task<ForumDto?> AddForum(ForumDto forum, string userId)
        {
            Guid parsedId = Guid.Parse(userId);
            var newForum = new Forum()
            {
                Id = Guid.NewGuid(),
                Name = forum.Name,
                ShortName = forum.ShortName,
                CreatedAt = DateTime.UtcNow,
                Description = forum.Description,
                ForumBanner = forum.ForumBanner,
                ForumImage = forum.ForumImage,
                CreatorId = parsedId
            };

            var insertedForum = await _unitOfWork.ForumRepo.InsertForumAsync(newForum);
            if (insertedForum == null)
                return null;

            await _unitOfWork.ForumRepo.InsertUserToForumAsync(newForum.Id, parsedId, ForumRole.Admin);
            await _unitOfWork.CommitAsync();

            return _mapper.Map<ForumDto>(insertedForum);
        }

        public async Task AddUserToForum(Guid forumId, Guid userId)
        {
            var forum = await _unitOfWork.ForumRepo.GetForumByIdAsync(forumId);
            if (forum == null)
                throw new ForumNotFoundException(forumId);

            await _unitOfWork.ForumRepo.InsertUserToForumAsync(forumId, userId, ForumRole.Member);
            await _unitOfWork.CommitAsync();
        }

        public async Task<MemberPermissionDto?> GetUserPermission(Guid forumId, Guid userId, CancellationToken cancellationToken = default)
        {
            var forum = await _unitOfWork.ForumRepo.GetForumByIdAsync(forumId, cancellationToken);
            if (forum == null)
                throw new ForumNotFoundException(forumId);

            var forumUser = await _unitOfWork.ForumRepo.GetForumUserAsync(forumId, userId, cancellationToken);
            if (forumUser == null)
                return null;

            var effective = await _permissionService.GetEffectivePermissionsAsync(forumId, userId, cancellationToken);

            return new MemberPermissionDto
            {
                UserId = userId,
                ForumId = forumId,
                Role = forumUser.Role,
                EffectivePermissions = effective,
                PermissionOverrides = forumUser.PermissionOverrides.HasValue
                    ? (ForumPermissionType)forumUser.PermissionOverrides.Value
                    : null,
            };
        }

        public async Task<List<ForumUserDto>> GetJoinedForums(Guid userId, CancellationToken cancellationToken = default)
        {
            return (await _unitOfWork.ForumRepo.GetJoinedForumsByUserIdAsync(userId, cancellationToken))
                .Select(f => new ForumUserDto()
                {
                    ForumId = f.ForumId,
                    Name = f.Forum.Name,
                    ForumImage = f.Forum.ForumImage,
                    Role = f.Role,
                }).ToList();
        }

        public async Task<List<ForumMemberDto>> GetForumMembers(Guid forumId, CancellationToken cancellationToken = default)
        {
            var forumUsers = await _unitOfWork.ForumRepo.GetForumUsersAsync(forumId, cancellationToken);
            if (forumUsers.Count == 0)
                return new List<ForumMemberDto>();

            var userIds = forumUsers.Select(f => f.UserId.ToString()).Distinct().ToList();

            Dictionary<Guid, UserDto> byId = new();
            try
            {
                await _retryPolicy.ExecuteAsync(async () =>
                {
                    var request = new ProfileRequest();
                    request.Id.AddRange(userIds);
                    var response = await _userProfileService.GetUserProfileAsync(request);
                    var users = _mapper.Map<List<UserDto>>(response.Profiles);
                    byId = users.ToDictionary(u => u.Id);
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to fetch forum member profiles from UserService");
            }

            var members = new List<ForumMemberDto>(forumUsers.Count);
            foreach (var fu in forumUsers)
            {
                if (!byId.TryGetValue(fu.UserId, out var profile))
                {
                    profile = new UserDto
                    {
                        Id = fu.UserId,
                        Username = fu.UserId.ToString(),
                        Avatar = string.Empty,
                        Bio = string.Empty,
                        BirthDay = default,
                        Gender = null,
                    };
                }

                var effective = await _permissionService.GetEffectivePermissionsAsync(forumId, fu.UserId, cancellationToken);
                members.Add(new ForumMemberDto
                {
                    Id = profile.Id,
                    Username = profile.Username,
                    Avatar = profile.Avatar,
                    Bio = profile.Bio,
                    BirthDay = profile.BirthDay,
                    Gender = profile.Gender,
                    Role = fu.Role,
                    EffectivePermissions = effective,
                    PermissionOverrides = fu.PermissionOverrides.HasValue
                        ? (ForumPermissionType)fu.PermissionOverrides.Value
                        : null,
                });
            }

            return members;
        }

        public async Task RemoveForumMember(Guid forumId, Guid targetUserId, Guid actingUserId)
        {
            var hasPermission = await _permissionService.HasPermissionAsync(
                forumId, actingUserId, ForumPermissionType.BanMember);
            if (!hasPermission)
                throw new UnauthorizedAccessException("You do not have permission to remove members.");

            var targetUser = await _unitOfWork.ForumRepo.GetForumUserAsync(forumId, targetUserId);
            if (targetUser == null)
                throw new InvalidOperationException("Target user is not a member of this forum.");

            if (targetUser.Role == ForumRole.Admin)
                throw new InvalidOperationException("Cannot remove the forum admin.");

            var actingUser = await _unitOfWork.ForumRepo.GetForumUserAsync(forumId, actingUserId);
            if (actingUser!.Role >= targetUser.Role)
                throw new UnauthorizedAccessException("Cannot remove a member with equal or higher role.");

            await _unitOfWork.ForumRepo.RemoveForumUserAsync(forumId, targetUserId);
            await _unitOfWork.CommitAsync();
            await _unitOfWork.ForumRepo.InvalidateCachedPermissionsAsync(forumId, targetUserId);
        }

        public async Task<List<ForumTagDto>> GetForumTagsAsync(Guid forumId, CancellationToken cancellationToken = default)
        {
            var forum = await _unitOfWork.ForumRepo.GetForumByIdAsync(forumId, cancellationToken);
            if (forum == null)
                throw new ForumNotFoundException(forumId);

            return await _unitOfWork.ForumRepo.GetForumTagCatalogAsync(forumId, cancellationToken);
        }

        public async Task<ForumTagDto> CreateForumTagAsync(Guid forumId, CreateForumTagRequest request, Guid actingUserId)
        {
            var forum = await _unitOfWork.ForumRepo.GetForumByIdAsync(forumId);
            if (forum == null)
                throw new ForumNotFoundException(forumId);

            var canManage = await _permissionService.HasPermissionAsync(
                forumId, actingUserId, ForumPermissionType.ManageTags);
            if (!canManage)
                throw new UnauthorizedAccessException("You do not have permission to manage forum tags.");

            var name = (request.Name ?? string.Empty).Trim();
            if (string.IsNullOrEmpty(name))
                throw new ArgumentException("Tag name is required.");
            if (name.Length > 64)
                throw new ArgumentException("Tag name must be at most 64 characters.");

            var color = string.IsNullOrWhiteSpace(request.Color) ? "#808080" : request.Color.Trim();
            if (color.Length > 32)
                throw new ArgumentException("Color must be at most 32 characters.");

            var added = await _unitOfWork.ForumRepo.AddForumTagCatalogIfNotExistsAsync(forumId, name, color);
            if (!added)
                throw new InvalidOperationException("A tag with this name already exists for this forum.");

            await _unitOfWork.CommitAsync();

            var tags = await _unitOfWork.ForumRepo.GetForumTagCatalogAsync(forumId);
            return tags.Single(t => t.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        }

        public async Task LeaveForumAsync(Guid forumId, Guid userId)
        {
            var forumUser = await _unitOfWork.ForumRepo.GetForumUserAsync(forumId, userId);
            if (forumUser == null)
                throw new InvalidOperationException("You are not a member of this forum.");

            if (forumUser.Role == ForumRole.Admin)
                throw new InvalidOperationException("Forum admin cannot leave. Transfer ownership first.");

            await _unitOfWork.ForumRepo.RemoveForumUserAsync(forumId, userId);
            await _unitOfWork.CommitAsync();
            await _unitOfWork.ForumRepo.InvalidateCachedPermissionsAsync(forumId, userId);
        }

        public async Task<List<ForumDto>> SearchForums(string query, CancellationToken cancellationToken = default)
        {
            var forums = await _unitOfWork.ForumRepo.SearchForumsAsync(query, cancellationToken);
            return _mapper.Map<List<ForumDto>>(forums);
        }

        public async ValueTask<List<ForumDto>> GetRecentVisitForums(Guid userId,
            CancellationToken cancellationToken = default)
        {
            var forums = await _unitOfWork.ForumRepo.GetRecentVisitForumsAsync(userId, cancellationToken);
            return _mapper.Map<List<ForumDto>>(forums);
        }
    }
}
