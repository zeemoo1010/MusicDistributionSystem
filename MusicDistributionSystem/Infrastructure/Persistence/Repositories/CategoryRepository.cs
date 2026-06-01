using Microsoft.EntityFrameworkCore;
using MusicDistributionSystem.Domain.Entities;
using MusicDistributionSystem.Application.Contracts.Repositories;

namespace MusicDistributionSystem.Persistence.Repositories
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
                .Include(category => category.MusicTracks)
                .AsNoTracking()
                .OrderBy(category => category.Name)
                .ToListAsync();
        }

        public Task<Category?> GetByIdAsync(Guid id)
        {
            return _context.Categories
                .Include(category => category.MusicTracks)
                .FirstOrDefaultAsync(category => category.Id == id);
        }

        public Task<Category?> GetByNameAsync(string name)
        {
            return _context.Categories.FirstOrDefaultAsync(category => category.Name == name);
        }

        public async Task AddAsync(Category category)
        {
            await _context.Categories.AddAsync(category);
        }

        public void Remove(Category category)
        {
            _context.Categories.Remove(category);
        }

        public Task<int> GetLinkedTrackCountAsync(Guid categoryId)
        {
            return _context.MusicTracks.CountAsync(track => track.CategoryId == categoryId);
        }

        public Task SaveChangesAsync()
        {
            return _context.SaveChangesAsync();
        }
    }
}
