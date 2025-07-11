using ForumService.ForumService.Application.Interfaces.Repositories;
using ForumService.ForumService.Application.Interfaces.UnitOfWork;
using ForumService.ForumService.Infrastructure.Data;
using ForumService.ForumService.Infrastructure.Repositories;
using StackExchange.Redis;


namespace ForumService.ForumService.Infrastructure.UnitOfWork
{
    public class UnitOfWork: IUnitOfWork
    {
        private readonly ForumDbContext _context;
        private readonly IConnectionMultiplexer _redis;

        public IForumThreadRepository ForumThreads { get; }
        public IForumRepository Forums { get; }
        
        public UnitOfWork(ForumDbContext context, IConnectionMultiplexer redis)
        {
            _context = context;
            _redis = redis;
            ForumThreads = new ForumThreadRepository(_context);
            Forums = new ForumRepository(_context, _redis);
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
