using Microsoft.EntityFrameworkCore;
using MusicDistributionSystem.Domain.Common;
using MusicDistributionSystem.Domain.Contracts.Interface;
using MusicDistributionSystem.Domain.Entities;
using MusicDistributionSystem.Domain.Enums;

namespace MusicDistributionSystem.Infrastructure.EntityFrameworkCore.Repositories
{
    public class AlbumRepository : IAlbumRepository
    {
        private readonly ApplicationDbContext _context;

        public AlbumRepository(ApplicationDbContext context) => _context = context;

        public async Task<IReadOnlyCollection<Album>> GetAllAsync()
        {
            return await _context.Albums
                .AsNoTracking()
                .Include(a => a.Artist)
                .Include(a => a.Category)
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();
        }

        public async Task<PaginatedResult<Album>> GetPagedAsync(string? searchTerm, Guid? categoryId, Guid? artistId, int page, int pageSize)
        {
            var query = _context.Albums
                .AsNoTracking()
                .Include(a => a.Artist)
                .Include(a => a.Category)
                .Include(a => a.Tracks)
                .Where(a => a.ApprovalStatus == ApprovalStatus.Approved);

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                query = query.Where(a => a.Title.Contains(searchTerm) || (a.Artist != null && a.Artist.Name.Contains(searchTerm)));
            }

            if (categoryId.HasValue)
            {
                query = query.Where(a => a.CategoryId == categoryId.Value);
            }

            if (artistId.HasValue)
            {
                query = query.Where(a => a.ArtistId == artistId.Value);
            }

            var totalCount = await query.CountAsync();
            var items = await query
                .OrderByDescending(a => a.IsFeatured)
                .ThenByDescending(a => a.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PaginatedResult<Album>
            {
                Items = items,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };
        }

        public Task<Album?> GetByIdAsync(Guid id, bool asNoTracking = true)
        {
            var query = _context.Albums
                .Include(a => a.Artist)
                .Include(a => a.Category)
                .Include(a => a.Tracks.OrderBy(t => t.TrackNumber))
                .Where(a => a.Id == id);

            if (asNoTracking) query = query.AsNoTracking();
            return query.FirstOrDefaultAsync();
        }

        public Task<Album?> GetBySlugAsync(string slug, bool asNoTracking = true)
        {
            var query = _context.Albums
                .Include(a => a.Artist)
                .Include(a => a.Category)
                .Include(a => a.Tracks.OrderBy(t => t.TrackNumber))
                .Where(a => a.Slug == slug);

            if (asNoTracking) query = query.AsNoTracking();
            return query.FirstOrDefaultAsync();
        }

        public async Task<IReadOnlyCollection<Album>> GetFeaturedAlbumsAsync(int take)
        {
            return await _context.Albums
                .AsNoTracking()
                .Include(a => a.Artist)
                .Include(a => a.Category)
                .Where(a => a.ApprovalStatus == ApprovalStatus.Approved && a.IsFeatured)
                .OrderByDescending(a => a.CreatedAt)
                .Take(take)
                .ToListAsync();
        }

        public async Task AddAsync(Album album) => await _context.Albums.AddAsync(album);
        public void Remove(Album album) => _context.Albums.Remove(album);
        public Task SaveChangesAsync() => _context.SaveChangesAsync();
    }
}
