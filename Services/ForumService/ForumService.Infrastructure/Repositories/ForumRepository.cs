using ForumService.ForumService.Application.Interfaces.Repositories;
using ForumService.ForumService.Domain.Entities;
using ForumService.ForumService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace ForumService.ForumService.Infrastructure.Repositories
{
    public class ForumRepository: IForumRepository
    {
        private readonly ForumDbContext _context;

        public ForumRepository(ForumDbContext context)
        {
            _context = context;
        }



        public async Task AddAsync()
        {

        }
    }
}
