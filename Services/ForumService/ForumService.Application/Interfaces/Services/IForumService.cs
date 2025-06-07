using ForumService.ForumService.Application.DTOs;

namespace ForumService.ForumService.Application.Interfaces.Services
{
    public interface IForumService
    {
        Task<ForumDto> AddForum(ForumDto forum);
        Task<ForumDto?> GetForum(Guid forumId);
        Task<IEnumerable<ForumDto>> GetForums();
    }
}