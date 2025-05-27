using ForumService.ForumService.Application.Interfaces.Repositories;

namespace ForumService.ForumService.Application.Interfaces.UnitOfWork
{
    public interface IUnitOfWork: IDisposable
    {
        public IForumRepository Forums { get; }
        public IForumThreadRepository ForumThreads { get; }
        public Task<int> CommitAsync();
    }
}
