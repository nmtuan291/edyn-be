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

        public async Task<ForumDto?> GetForum(Guid forumId)
        {
            Forum? forum = await _unitOfWork.Forums.GetForumByIdAsync(forumId);

            if (forum == null)
            {
                return null;
            }

            return new ForumDto
            {
                Id = forumId,
                Name = forum.Name,
                ShortName = forum.ShortName,
                CreatedAt = forum.CreatedAt,
                Description = forum.Description,
                ForumBanner = forum.ForumBanner,
                ForumImage = forum.ForumImage,
                CreatorId = forum.CreatorId
            };
        }

        public async Task<IEnumerable<ForumDto>> GetForums()
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
            });
        }

        public async Task<ForumDto> AddForum(ForumDto forum)
        {
            Forum newForum = new Forum
            {
                Id = Guid.NewGuid(),
                Name = forum.Name,
                ShortName = forum.ShortName,
                CreatedAt = forum.CreatedAt,
                Description = forum.Description,
                ForumBanner = forum.ForumBanner,
                ForumImage = forum.ForumImage,
                CreatorId = forum.CreatorId,
            };
            
            var insertedForum = await _unitOfWork.Forums.InsertForumAsync(newForum);
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
    }
}
