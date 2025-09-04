using AutoMapper;
using ForumService.ForumService.Application.DTOs;
using ForumService.ForumService.Application.Exceptions;
using ForumService.ForumService.Application.Interfaces.UnitOfWork;
using ForumService.ForumService.Domain.Entities;
using ForumService.ForumService.Application.Interfaces.Services;
using Grpc.Core;
using Polly;
using Polly.Retry;
using UserService.Grpc;

namespace ForumService.ForumService.Application
{
    public class ForumService : IForumService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly UserProfileService.UserProfileServiceClient _userProfileService;
        private readonly AsyncRetryPolicy _retryPolicy;

        public ForumService(IUnitOfWork unitOfWork, IMapper mapper,  UserProfileService.UserProfileServiceClient userProfileService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _userProfileService = userProfileService;
            _retryPolicy = Policy
                .Handle<Exception>()
                .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));
        }

        public async Task<ForumDto?> GetForum(string forumName)
        {
            Forum? forum = await _unitOfWork.ForumRepo.GetForumByNameAsync(forumName);
            if (forum == null)
                return null;

            return _mapper.Map<ForumDto>(forum);
        }

        public async Task<List<ForumDto>> GetForums()
        {
            var forums = await _unitOfWork.ForumRepo.GetForumsAsync();

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
            
            await _unitOfWork.ForumRepo.InsertUserToForumAsync(newForum.Id, parsedId, "--------", true);
            
            await _unitOfWork.CommitAsync();

            return _mapper.Map<ForumDto>(insertedForum);
        }

        public async Task AddUserToForum(Guid forumId, Guid userId)
        {
            var forum = await _unitOfWork.ForumRepo.GetForumByIdAsync(forumId);
            if (forum == null)
                throw new ForumNotFoundException(forumId);
                
            await _unitOfWork.ForumRepo.InsertUserToForumAsync(forumId, userId, "--------", false);
        }

        public async Task<string?> GetUserPermission(Guid forumId, Guid userId)
        {
            var forum = await _unitOfWork.ForumRepo.GetForumByIdAsync(forumId);
            if (forum == null)
                throw new ForumNotFoundException(forumId);
            
            return await _unitOfWork.ForumRepo.GetUserPermissionAsync(forumId, userId);
        }

        public async Task<List<ForumUserDto>> GetJoinedForums(Guid userId)
        {
            return (await _unitOfWork.ForumRepo.GetJoinedForumsByUserIdAsync(userId))
                .Select(f => new ForumUserDto()
                {
                    ForumId = f.ForumId,
                    Name = f.Forum.Name,
                    ForumImage = f.Forum.ForumImage,
                }).ToList();
        }

        public async Task<List<UserDto>> GetForumMembers(Guid forumId)
        {
            var userIds = (await _unitOfWork.ForumRepo
                .GetForumUsersAsync(forumId))
                .Select(f => f.UserId.ToString())
                .ToList();
            
            if (userIds.Count == 0)
                return new List<UserDto>();

            try
            {
                List<UserDto> users = new List<UserDto>();
                await _retryPolicy.ExecuteAsync(async () =>
                {
                    var request = new ProfileRequest();
                    request.Id.AddRange(userIds);
                    var response = await _userProfileService.GetUserProfileAsync(request);
                    users = _mapper.Map<List<UserDto>>(response.Profiles);
                });

                return users;
            }
            catch (Exception ex)
            {
                return new List<UserDto>();
            }
        }
    }
}
