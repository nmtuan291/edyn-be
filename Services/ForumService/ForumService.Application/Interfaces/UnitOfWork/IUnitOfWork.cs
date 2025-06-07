using ForumService.ForumService.Application.Interfaces.Repositories;

namespace ForumService.ForumService.Application.Interfaces.UnitOfWork
{
    public interface IUnitOfWork: IDisposable
    {
        IForumRepository Forums { get; }
        IForumThreadRepository ForumThreads { get; }
        Task<int> CommitAsync();
    }
}
