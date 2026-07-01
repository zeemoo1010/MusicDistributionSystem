using Microsoft.EntityFrameworkCore;
using MusicDistributionSystem.Domain.Contracts.Interface;
using MusicDistributionSystem.Domain.Entities;

namespace MusicDistributionSystem.Infrastructure.EntityFrameworkCore.Repositories
{
    public class TagRepository : ITagRepository
    {
        private readonly ApplicationDbContext _context;

        public TagRepository(ApplicationDbContext context) => _context = context;

        public async Task<IReadOnlyCollection<Tag>> GetAllAsync()
        {
            return await _context.Tags
                .AsNoTracking()
                .OrderBy(t => t.Name)
                .ToListAsync();
        }

        public Task<Tag?> GetByNameAsync(string name)
        {
            return _context.Tags.FirstOrDefaultAsync(t => t.Name == name);
        }

        public async Task AddAsync(Tag tag) => await _context.Tags.AddAsync(tag);
        public Task SaveChangesAsync() => _context.SaveChangesAsync();
    }
}