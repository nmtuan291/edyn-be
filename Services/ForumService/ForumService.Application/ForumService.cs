using AutoMapper;
using ForumService.ForumService.Application.DTOs;
using ForumService.ForumService.Application.Exceptions;
using ForumService.ForumService.Application.Interfaces.UnitOfWork;
using ForumService.ForumService.Domain.Entities;
using ForumService.ForumService.Application.Interfaces.Services;

namespace ForumService.ForumService.Application
{
    public class ForumService : IForumService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public ForumService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
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
    }
}
