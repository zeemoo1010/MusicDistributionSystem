using Microsoft.EntityFrameworkCore;
using MusicDistributionSystem.Domain.Common;
using MusicDistributionSystem.Domain.Contracts.Interface;
using MusicDistributionSystem.Domain.Entities;
using MusicDistributionSystem.Domain.Enums;

namespace MusicDistributionSystem.Infrastructure.EntityFrameworkCore.Repositories
{
    public class VideoRepository : IVideoRepository
    {
        private readonly ApplicationDbContext _context;

        public VideoRepository(ApplicationDbContext context) => _context = context;

        public async Task<IReadOnlyCollection<Video>> GetApprovedVideosAsync(string? searchTerm, Guid? categoryId)
        {
            var query = BuildApprovedQuery(searchTerm, categoryId);
            return await query.ToListAsync();
        }

        public async Task<PaginatedResult<Video>> GetApprovedVideosPagedAsync(string? searchTerm, Guid? categoryId, int page, int pageSize)
        {
            var query = BuildApprovedQuery(searchTerm, categoryId);
            var totalCount = await query.CountAsync();
            var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

            return new PaginatedResult<Video>
            {
                Items = items,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };
        }

        public async Task<IReadOnlyCollection<Video>> GetFeaturedVideosAsync(int take)
        {
            return await _context.Videos
                .AsNoTracking()
                .Include(v => v.Category)
                .Include(v => v.Artist)
                .Where(v => v.ApprovalStatus == ApprovalStatus.Approved && v.IsFeatured)
                .OrderByDescending(v => v.CreatedAt)
                .Take(take)
                .ToListAsync();
        }

        public async Task<IReadOnlyCollection<Video>> GetLatestApprovedVideosAsync(int take)
        {
            return await _context.Videos
                .AsNoTracking()
                .Include(v => v.Category)
                .Include(v => v.Artist)
                .Where(v => v.ApprovalStatus == ApprovalStatus.Approved)
                .OrderByDescending(v => v.CreatedAt)
                .Take(take)
                .ToListAsync();
        }

        public Task<Video?> GetApprovedVideoByIdAsync(Guid id, bool asNoTracking = true)
        {
            var query = _context.Videos
                .Include(v => v.Category)
                .Include(v => v.Artist)
                .Include(v => v.Comments.Where(c => c.IsApproved))
                    .ThenInclude(c => c.User)
                .Include(v => v.Likes)
                .Where(v => v.Id == id && v.ApprovalStatus == ApprovalStatus.Approved);

            if (asNoTracking) query = query.AsNoTracking();
            return query.FirstOrDefaultAsync();
        }

        public Task<Video?> GetApprovedVideoBySlugAsync(string slug, bool asNoTracking = true)
        {
            var query = _context.Videos
                .Include(v => v.Category)
                .Include(v => v.Artist)
                .Include(v => v.Comments.Where(c => c.IsApproved))
                    .ThenInclude(c => c.User)
                .Include(v => v.Likes)
                .Where(v => v.Slug == slug && v.ApprovalStatus == ApprovalStatus.Approved);

            if (asNoTracking) query = query.AsNoTracking();
            return query.FirstOrDefaultAsync();
        }

        public Task<Video?> GetByIdAsync(Guid id, bool asNoTracking = true)
        {
            var query = _context.Videos
                .Include(v => v.Category)
                .Include(v => v.Artist)
                .Include(v => v.UploadedByUser)
                .Where(v => v.Id == id);

            if (asNoTracking) query = query.AsNoTracking();
            return query.FirstOrDefaultAsync();
        }

        public async Task<IReadOnlyCollection<Video>> GetPendingVideosAsync()
        {
            return await _context.Videos
                .AsNoTracking()
                .Include(v => v.Category)
                .Include(v => v.Artist)
                .Include(v => v.UploadedByUser)
                .Where(v => v.ApprovalStatus == ApprovalStatus.Pending)
                .OrderBy(v => v.CreatedAt)
                .ToListAsync();
        }

        public async Task<IReadOnlyCollection<Video>> GetVideosByUploaderAsync(Guid uploaderUserId)
        {
            return await _context.Videos
                .AsNoTracking()
                .Include(v => v.Category)
                .Where(v => v.UploadedByUserId == uploaderUserId)
                .OrderByDescending(v => v.CreatedAt)
                .ToListAsync();
        }

        public Task<int> CountApprovedAsync() => _context.Videos.CountAsync(v => v.ApprovalStatus == ApprovalStatus.Approved);
        public Task<int> CountPendingAsync() => _context.Videos.CountAsync(v => v.ApprovalStatus == ApprovalStatus.Pending);

        public async Task AddAsync(Video video) => await _context.Videos.AddAsync(video);
        public void Remove(Video video) => _context.Videos.Remove(video);
        public Task SaveChangesAsync() => _context.SaveChangesAsync();

        private IQueryable<Video> BuildApprovedQuery(string? searchTerm, Guid? categoryId)
        {
            var query = _context.Videos
                .AsNoTracking()
                .Include(v => v.Category)
                .Include(v => v.Artist)
                .Where(v => v.ApprovalStatus == ApprovalStatus.Approved);

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                query = query.Where(v => v.Title.Contains(searchTerm) || v.ArtistName.Contains(searchTerm) || (v.Description != null && v.Description.Contains(searchTerm)));
            }

            if (categoryId.HasValue)
            {
                query = query.Where(v => v.CategoryId == categoryId.Value);
            }

            return query.OrderByDescending(v => v.IsFeatured).ThenByDescending(v => v.CreatedAt);
        }
    }
}
