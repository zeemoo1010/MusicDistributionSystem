using Microsoft.EntityFrameworkCore;
using MusicDistributionSystem.Domain.Entities;
using MusicDistributionSystem.Application.Contracts.Repositories;

namespace MusicDistributionSystem.Infrastructure.Persistence.Repositories
{
    public class CategoryRepository : ICategoryRepository
    {
        private readonly ApplicationDbContext _context;

        public CategoryRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IReadOnlyCollection<Category>> GetAllAsync()
        {
            return await _context.Categories
                .AsNoTracking()
                .OrderBy(category => category.Name)
                .ToListAsync();
        }
    }
}
