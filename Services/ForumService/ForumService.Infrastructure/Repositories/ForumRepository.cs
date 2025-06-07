using ForumService.ForumService.Application.Interfaces.Repositories;
using ForumService.ForumService.Domain.Entities;
using ForumService.ForumService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;

namespace ForumService.ForumService.Infrastructure.Repositories
{
    public class ForumRepository: IForumRepository
    {
        private readonly ForumDbContext _context;

        public ForumRepository(ForumDbContext context)
        {
            _context = context;
        }

        public async Task<Forum?> GetForumByIdAsync(Guid forumId)
        {
            return await _context.Forums
                .Where(f => f.Id == forumId)
                .SingleOrDefaultAsync();
        }

        public async Task<IEnumerable<Forum>> GetForumsAsync()
        {
            return await _context.Forums.ToListAsync();
        }
        
        public async Task<Forum> InsertForumAsync(Forum forum)
        {
            await _context.Forums.AddAsync(forum);
            return forum;
        }
    }
}
