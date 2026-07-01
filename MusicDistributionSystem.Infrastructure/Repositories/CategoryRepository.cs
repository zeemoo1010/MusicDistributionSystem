using Microsoft.EntityFrameworkCore;
using MusicDistributionSystem.Domain.Contracts.Interface;
using MusicDistributionSystem.Domain.Entities;

namespace MusicDistributionSystem.Infrastructure.EntityFrameworkCore.Repositories
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

        public Task<Category?> GetByIdAsync(Guid id)
        {
            return _context.Categories
                .AsNoTracking()
                .FirstOrDefaultAsync(category => category.Id == id);
        }

        public Task<Category?> GetByIdWithTracksAsync(Guid id)
        {
            return _context.Categories
                .AsNoTracking()
                .Include(c => c.MusicTracks)
                .Include(c => c.MediaAssets)
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
