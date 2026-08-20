using Microsoft.EntityFrameworkCore;
using MusicDistributionSystem.Domain.Common;
using MusicDistributionSystem.Domain.Contracts.Interface;
using MusicDistributionSystem.Domain.Entities;

namespace MusicDistributionSystem.Infrastructure.EntityFrameworkCore.Repositories
{
    public class ArtistRepository : IArtistRepository
    {
        private readonly ApplicationDbContext _context;

        public ArtistRepository(ApplicationDbContext context) => _context = context;

        public async Task<IReadOnlyCollection<Artist>> GetAllAsync()
        {
            return await _context.Artists
                .AsNoTracking()
                .OrderBy(a => a.Name)
                .ToListAsync();
        }

        public async Task<PaginatedResult<Artist>> GetPagedAsync(string? searchTerm, int page, int pageSize)
        {
            var query = _context.Artists.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                query = query.Where(a => a.Name.Contains(searchTerm) || (a.Bio != null && a.Bio.Contains(searchTerm)));
            }

            var totalCount = await query.CountAsync();
            var items = await query
                .OrderByDescending(a => a.IsVerified)
                .ThenBy(a => a.Name)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PaginatedResult<Artist>
            {
                Items = items,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };
        }

        public Task<Artist?> GetByIdAsync(Guid id, bool asNoTracking = true)
        {
            var query = _context.Artists
                .Include(a => a.Albums)
                .Include(a => a.Tracks)
                .Include(a => a.Videos)
                .Where(a => a.Id == id);

            if (asNoTracking) query = query.AsNoTracking();
            return query.FirstOrDefaultAsync();
        }

        public Task<Artist?> GetBySlugAsync(string slug, bool asNoTracking = true)
        {
            var query = _context.Artists
                .Include(a => a.Albums)
                .Include(a => a.Tracks)
                .Include(a => a.Videos)
                .Where(a => a.Slug == slug);

            if (asNoTracking) query = query.AsNoTracking();
            return query.FirstOrDefaultAsync();
        }

        public async Task<IReadOnlyCollection<Artist>> GetFeaturedArtistsAsync(int take)
        {
            return await _context.Artists
                .AsNoTracking()
                .OrderByDescending(a => a.IsVerified)
                .ThenBy(a => a.Name)
                .Take(take)
                .ToListAsync();
        }

        public async Task AddAsync(Artist artist) => await _context.Artists.AddAsync(artist);
        public void Remove(Artist artist) => _context.Artists.Remove(artist);
        public Task SaveChangesAsync() => _context.SaveChangesAsync();
    }
}
