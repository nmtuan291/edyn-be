using ForumService.ForumService.Application.Interfaces.Repositories;
using ForumService.ForumService.Application.Interfaces.UnitOfWork;
using ForumService.ForumService.Infrastructure.Data;
using ForumService.ForumService.Infrastructure.Repositories;

namespace ForumService.ForumService.Infrastructure.UnitOfWork
{
    public class UnitOfWork: IUnitOfWork
    {
        private readonly ForumDbContext _context;

        public IForumThreadRepository ForumThreads { get; }
        public IForumRepository Forums { get; }
        
        public UnitOfWork(ForumDbContext context)
        {
            _context = context;
            ForumThreads = new ForumThreadRepository(_context);
            Forums = new ForumRepository(_context);
        }

        public async Task<int> CommitAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
