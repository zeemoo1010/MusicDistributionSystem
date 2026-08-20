using Microsoft.EntityFrameworkCore;
using MusicDistributionSystem.Domain.Common;
using MusicDistributionSystem.Domain.Contracts.Interface;
using MusicDistributionSystem.Domain.Entities;
using MusicDistributionSystem.Domain.Enums;

namespace MusicDistributionSystem.Infrastructure.EntityFrameworkCore.Repositories
{
    public class ImageAssetRepository : IImageAssetRepository
    {
        private readonly ApplicationDbContext _context;

        public ImageAssetRepository(ApplicationDbContext context) => _context = context;

        public async Task<IReadOnlyCollection<ImageAsset>> GetApprovedImagesAsync(string? searchTerm, Guid? categoryId)
        {
            var query = BuildApprovedQuery(searchTerm, categoryId);
            return await query.ToListAsync();
        }

        public async Task<PaginatedResult<ImageAsset>> GetApprovedImagesPagedAsync(string? searchTerm, Guid? categoryId, int page, int pageSize)
        {
            var query = BuildApprovedQuery(searchTerm, categoryId);
            var totalCount = await query.CountAsync();
            var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

            return new PaginatedResult<ImageAsset>
            {
                Items = items,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };
        }

        public Task<ImageAsset?> GetByIdAsync(Guid id, bool asNoTracking = true)
        {
            var query = _context.ImageAssets
                .Include(i => i.Category)
                .Include(i => i.UploadedByUser)
                .Where(i => i.Id == id);

            if (asNoTracking) query = query.AsNoTracking();
            return query.FirstOrDefaultAsync();
        }

        public Task<ImageAsset?> GetBySlugAsync(string slug, bool asNoTracking = true)
        {
            var query = _context.ImageAssets
                .Include(i => i.Category)
                .Include(i => i.UploadedByUser)
                .Where(i => i.Slug == slug);

            if (asNoTracking) query = query.AsNoTracking();
            return query.FirstOrDefaultAsync();
        }

        public async Task<IReadOnlyCollection<ImageAsset>> GetFeaturedImagesAsync(int take)
        {
            return await _context.ImageAssets
                .AsNoTracking()
                .Include(i => i.Category)
                .Where(i => i.ApprovalStatus == ApprovalStatus.Approved && i.IsFeatured)
                .OrderByDescending(i => i.CreatedAt)
                .Take(take)
                .ToListAsync();
        }

        public Task<int> CountApprovedAsync() => _context.ImageAssets.CountAsync(i => i.ApprovalStatus == ApprovalStatus.Approved);

        public async Task AddAsync(ImageAsset image) => await _context.ImageAssets.AddAsync(image);
        public void Remove(ImageAsset image) => _context.ImageAssets.Remove(image);
        public Task SaveChangesAsync() => _context.SaveChangesAsync();

        private IQueryable<ImageAsset> BuildApprovedQuery(string? searchTerm, Guid? categoryId)
        {
            var query = _context.ImageAssets
                .AsNoTracking()
                .Include(i => i.Category)
                .Where(i => i.ApprovalStatus == ApprovalStatus.Approved);

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                query = query.Where(i => i.Title.Contains(searchTerm) || (i.Description != null && i.Description.Contains(searchTerm)));
            }

            if (categoryId.HasValue)
            {
                query = query.Where(i => i.CategoryId == categoryId.Value);
            }

            return query.OrderByDescending(i => i.IsFeatured).ThenByDescending(i => i.CreatedAt);
        }
    }
}
