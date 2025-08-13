using ForumService.ForumService.Application.DTOs;
using ForumService.ForumService.Application.Interfaces.UnitOfWork;
using ForumService.ForumService.Domain.Entities;
using ForumService.ForumService.Application.Interfaces.Services;

namespace ForumService.ForumService.Application
{
    public class ForumService : IForumService
    {
        private readonly IUnitOfWork _unitOfWork;

        public ForumService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<ForumDto?> GetForum(string forumName)
        {
            Forum? forum = await _unitOfWork.Forums.GetForumByNameAsync(forumName);

            if (forum == null)
            {
                return null;
            }

            return new ForumDto
            {
                Id = forum.Id,
                Name = forum.Name,
                ShortName = forum.ShortName,
                CreatedAt = forum.CreatedAt,
                Description = forum.Description,
                ForumBanner = forum.ForumBanner,
                ForumImage = forum.ForumImage,
                CreatorId = forum.CreatorId
            };
        }

        public async Task<List<ForumDto>> GetForums()
        {
            var forums = await _unitOfWork.Forums.GetForumsAsync();

            return forums.Select(forum => new ForumDto
            {
                Id = forum.Id,
                Name = forum.Name,
                ShortName = forum.ShortName,
                CreatedAt = forum.CreatedAt,
                Description = forum.Description,
                ForumBanner = forum.ForumBanner,
                ForumImage = forum.ForumImage,
                CreatorId = forum.CreatorId
            }).ToList();
        }

        public async Task<ForumDto?> AddForum(ForumDto forum, string userId)
        {
            Guid parsedId = Guid.Parse(userId);
            Forum newForum = new Forum
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
            
            var insertedForum = await _unitOfWork.Forums.InsertForumAsync(newForum);
            if (insertedForum == null)
                return null;
            
            await _unitOfWork.Forums.InsertUserToForumAsync(newForum.Id, parsedId, "--------", true);
            
            await _unitOfWork.CommitAsync();

            return new ForumDto 
            {
                Id = insertedForum.Id,
                Name = insertedForum.Name,
                ShortName = insertedForum.ShortName,
                CreatedAt = insertedForum.CreatedAt,
                Description = insertedForum.Description,
                ForumBanner = insertedForum.ForumBanner,
                ForumImage = insertedForum.ForumImage,
                CreatorId = insertedForum.CreatorId
            };
        }

        public async Task AddUserToForum(Guid forumId, Guid userId)
        {
            await _unitOfWork.Forums.InsertUserToForumAsync(forumId, userId, "--------", false);
        }

        public async Task<string?> GetUserPermission(Guid forumId, Guid userId)
        {
            return await _unitOfWork.Forums.GetUserPermissionAsync(forumId, userId);
        }
    }
}
