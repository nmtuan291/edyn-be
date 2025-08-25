using AutoMapper;
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

        public IThreadRepository ThreadRepo { get; }
        public IForumRepository ForumRepo { get; }
        public IVoteRepository VoteRepo { get; }
        public ICommentRepository CommentRepo { get; }
        
        public UnitOfWork(ForumDbContext context, IConnectionMultiplexer redis, IMapper mapper)
        {
            _context = context;
            _redis = redis;
            ThreadRepo = new ThreadRepository(_context, _redis, mapper);
            ForumRepo = new ForumRepository(_context, _redis, mapper);
            VoteRepo = new VoteRepository(_context, _redis, mapper);
            CommentRepo = new CommentRepository(_context, mapper);
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
