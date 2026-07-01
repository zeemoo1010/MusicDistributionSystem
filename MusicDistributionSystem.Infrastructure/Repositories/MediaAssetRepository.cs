using Microsoft.EntityFrameworkCore;
using MusicDistributionSystem.Domain.Contracts.Interface;
using MusicDistributionSystem.Domain.Entities;
using MusicDistributionSystem.Domain.Enums;

namespace MusicDistributionSystem.Infrastructure.EntityFrameworkCore.Repositories
{
    public class MediaAssetRepository : IMediaAssetRepository
    {
        private readonly ApplicationDbContext _context;

        public MediaAssetRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IReadOnlyCollection<MediaAsset>> GetApprovedAsync(
            string? searchTerm, Guid? categoryId, MediaType? type, int page, int pageSize)
        {
            var query = _context.MediaAssets
                .AsNoTracking()
                .Include(a => a.Category)
                .Where(a => a.ApprovalStatus == ApprovalStatus.Approved);

            if (!string.IsNullOrWhiteSpace(searchTerm))
                query = query.Where(a => a.Title.Contains(searchTerm) || a.UploadedByName.Contains(searchTerm));

            if (categoryId.HasValue)
                query = query.Where(a => a.CategoryId == categoryId.Value);

            if (type.HasValue)
                query = query.Where(a => a.Type == type.Value);

            return await query
                .OrderByDescending(a => a.IsFeatured)
                .ThenByDescending(a => a.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<IReadOnlyCollection<MediaAsset>> GetLatestAsync(int take)
        {
            return await _context.MediaAssets
                .AsNoTracking()
                .Include(a => a.Category)
                .Where(a => a.ApprovalStatus == ApprovalStatus.Approved)
                .OrderByDescending(a => a.IsFeatured)
                .ThenByDescending(a => a.CreatedAt)
                .Take(take)
                .ToListAsync();
        }

        public async Task<MediaAsset?> GetApprovedByIdAsync(Guid id, bool asNoTracking = true)
        {
            var query = _context.MediaAssets
                .Include(a => a.Category)
                .Where(a => a.Id == id && a.ApprovalStatus == ApprovalStatus.Approved);

            if (asNoTracking) query = query.AsNoTracking();

            return await query.FirstOrDefaultAsync();
        }

        public Task<MediaAsset?> GetByIdAsync(Guid id, bool asNoTracking = true)
        {
            var query = _context.MediaAssets
                .Where(a => a.Id == id);

            if (asNoTracking) query = query.AsNoTracking();

            return query
                .Include(a => a.Category)
                .Include(a => a.UploadedByUser)
                .FirstOrDefaultAsync();
        }

        public async Task<IReadOnlyCollection<MediaAsset>> GetPendingAsync()
        {
            return await _context.MediaAssets
                .AsNoTracking()
                .Include(a => a.Category)
                .Include(a => a.UploadedByUser)
                .Where(a => a.ApprovalStatus == ApprovalStatus.Pending)
                .OrderBy(a => a.CreatedAt)
                .ToListAsync();
        }

        public async Task<IReadOnlyCollection<MediaAsset>> GetByUploaderAsync(Guid uploaderUserId)
        {
            return await _context.MediaAssets
                .AsNoTracking()
                .Include(a => a.Category)
                .Where(a => a.UploadedByUserId == uploaderUserId)
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();
        }

        public Task<int> CountByStatusAsync(ApprovalStatus status)
        {
            return _context.MediaAssets.CountAsync(a => a.ApprovalStatus == status);
        }

        public Task<int> CountByTypeAsync(MediaType type)
        {
            return _context.MediaAssets.CountAsync(a => a.Type == type);
        }

        public async Task<int> GetTotalDownloadsAsync()
        {
            return await _context.MediaAssets.SumAsync(a => (int?)a.DownloadCount) ?? 0;
        }

        public async Task AddAsync(MediaAsset asset)
        {
            await _context.MediaAssets.AddAsync(asset);
        }

        public void Remove(MediaAsset asset)
        {
            _context.MediaAssets.Remove(asset);
        }

        public Task SaveChangesAsync()
        {
            return _context.SaveChangesAsync();
        }
    }
}