using ForumService.ForumService.Application.Interfaces.UnitOfWork;
using ForumService.ForumService.Infrastructure.Repositories;

namespace ForumService.ForumService.Application
{
    public class ForumThreadService
    {
        private readonly IUnitOfWork _unitOfWork;

        public ForumThreadService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        // Need to return something? dto ? entity?
        public Task GetThreadByForumId(Guid forumId)
        {
            var threads = _unitOfWork.ForumThreads.GetThreadsByForumIdAsync(forumId);
            return Task.FromResult(threads);
        }
    }
}
