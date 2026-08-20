using Microsoft.EntityFrameworkCore;
using MusicDistributionSystem.Domain.Common;
using MusicDistributionSystem.Domain.Contracts.Interface;
using MusicDistributionSystem.Domain.Entities;
using MusicDistributionSystem.Domain.Enums;

namespace MusicDistributionSystem.Infrastructure.EntityFrameworkCore.Repositories
{
    public class MusicRepository : IMusicRepository
    {
        private readonly ApplicationDbContext _context;

        public MusicRepository(ApplicationDbContext context) => _context = context;

        public async Task<IReadOnlyCollection<MusicTrack>> GetApprovedTracksAsync(string? searchTerm, Guid? categoryId)
        {
            var query = BuildApprovedQuery(searchTerm, categoryId);
            return await query.ToListAsync();
        }

        public async Task<PaginatedResult<MusicTrack>> GetApprovedTracksPagedAsync(string? searchTerm, Guid? categoryId, int page, int pageSize)
        {
            var query = BuildApprovedQuery(searchTerm, categoryId);
            return await ToPaginatedAsync(query, page, pageSize);
        }

        public async Task<IReadOnlyCollection<MusicTrack>> GetLatestApprovedTracksAsync(int take)
        {
            return await _context.MusicTracks
                .AsNoTracking()
                .Include(track => track.Category)
                .Include(track => track.Album)
                .Include(track => track.ArtistEntity)
                .Where(track => track.ApprovalStatus == ApprovalStatus.Approved)
                .OrderByDescending(track => track.IsFeatured)
                .ThenByDescending(track => track.CreatedAt)
                .Take(take)
                .ToListAsync();
        }

        public async Task<IReadOnlyCollection<MusicTrack>> GetTrendingTracksAsync(int take)
        {
            return await _context.MusicTracks
                .AsNoTracking()
                .Include(track => track.Category)
                .Include(track => track.Album)
                .Include(track => track.ArtistEntity)
                .Where(track => track.ApprovalStatus == ApprovalStatus.Approved)
                .OrderByDescending(track => track.PlayCount + (track.DownloadCount * 2))
                .Take(take)
                .ToListAsync();
        }

        public async Task<IReadOnlyCollection<MusicTrack>> GetTopChartTracksAsync(int take)
        {
            return await _context.MusicTracks
                .AsNoTracking()
                .Include(track => track.Category)
                .Include(track => track.Album)
                .Include(track => track.ArtistEntity)
                .Where(track => track.ApprovalStatus == ApprovalStatus.Approved)
                .OrderByDescending(track => track.DownloadCount)
                .Take(take)
                .ToListAsync();
        }

        public async Task<MusicTrack?> GetApprovedTrackByIdAsync(Guid id, bool asNoTracking = true)
        {
            var query = _context.MusicTracks
                .Include(track => track.Category)
                .Include(track => track.Album)
                .Include(track => track.ArtistEntity)
                .Include(track => track.Comments.Where(c => c.IsApproved))
                    .ThenInclude(c => c.User)
                .Include(track => track.Likes)
                .Where(track => track.Id == id && track.ApprovalStatus == ApprovalStatus.Approved);

            if (asNoTracking) query = query.AsNoTracking();
            return await query.FirstOrDefaultAsync();
        }

        public async Task<MusicTrack?> GetApprovedTrackBySlugAsync(string slug, bool asNoTracking = true)
        {
            var query = _context.MusicTracks
                .Include(track => track.Category)
                .Include(track => track.Album)
                .Include(track => track.ArtistEntity)
                .Include(track => track.Comments.Where(c => c.IsApproved))
                    .ThenInclude(c => c.User)
                .Include(track => track.Likes)
                .Where(track => track.Slug == slug && track.ApprovalStatus == ApprovalStatus.Approved);

            if (asNoTracking) query = query.AsNoTracking();
            return await query.FirstOrDefaultAsync();
        }


        public async Task<MusicTrack?> GetByIdAsync(Guid id, bool asNoTracking = true)
        {
            var query = _context.MusicTracks
                .Include(track => track.Category)
                .Include(track => track.UploadedByUser)
                .Where(track => track.Id == id);

            if (asNoTracking) query = query.AsNoTracking();
            return await query.FirstOrDefaultAsync();
        }

        public async Task<IReadOnlyCollection<MusicTrack>> GetPendingTracksAsync()
        {
            return await _context.MusicTracks
                .AsNoTracking()
                .Include(track => track.Category)
                .Include(track => track.UploadedByUser)
                .Where(track => track.ApprovalStatus == ApprovalStatus.Pending)
                .OrderBy(track => track.CreatedAt)
                .ToListAsync();
        }

        public async Task<PaginatedResult<MusicTrack>> GetPendingTracksPagedAsync(int page, int pageSize)
        {
            var query = _context.MusicTracks
                .AsNoTracking()
                .Include(track => track.Category)
                .Include(track => track.UploadedByUser)
                .Where(track => track.ApprovalStatus == ApprovalStatus.Pending)
                .OrderBy(track => track.CreatedAt);

            return await ToPaginatedAsync(query, page, pageSize);
        }

        public async Task<IReadOnlyCollection<MusicTrack>> GetTracksByUploaderAsync(Guid uploaderUserId)
        {
            return await _context.MusicTracks
                .AsNoTracking()
                .Include(track => track.Category)
                .Where(track => track.UploadedByUserId == uploaderUserId)
                .OrderByDescending(track => track.CreatedAt)
                .ToListAsync();
        }

        public async Task<PaginatedResult<MusicTrack>> GetTracksByUploaderPagedAsync(Guid uploaderUserId, int page, int pageSize)
        {
            var query = _context.MusicTracks
                .AsNoTracking()
                .Include(track => track.Category)
                .Where(track => track.UploadedByUserId == uploaderUserId)
                .OrderByDescending(track => track.CreatedAt);

            return await ToPaginatedAsync(query, page, pageSize);
        }

        public Task<int> CountAllAsync() => _context.MusicTracks.CountAsync();
        public Task<int> CountApprovedAsync() => _context.MusicTracks.CountAsync(t => t.ApprovalStatus == ApprovalStatus.Approved);
        public Task<int> CountRejectedAsync() => _context.MusicTracks.CountAsync(t => t.ApprovalStatus == ApprovalStatus.Rejected);

        public Task<int> CountPremiumApprovedAsync() => _context.MusicTracks.CountAsync(t =>
            t.ApprovalStatus == ApprovalStatus.Approved && t.AccessLevel != ContentAccessLevel.Free);

        public async Task<int> GetTotalDownloadsAsync() => await _context.MusicTracks.SumAsync(t => (int?)t.DownloadCount) ?? 0;

        public async Task AddAsync(MusicTrack track) => await _context.MusicTracks.AddAsync(track);
        public void Remove(MusicTrack track) => _context.MusicTracks.Remove(track);
        public async Task AddDownloadRecordAsync(DownloadRecord downloadRecord) => await _context.DownloadRecords.AddAsync(downloadRecord);
        public Task SaveChangesAsync() => _context.SaveChangesAsync();

        private IQueryable<MusicTrack> BuildApprovedQuery(string? searchTerm, Guid? categoryId)
        {
            var query = _context.MusicTracks
                .AsNoTracking()
                .Include(t => t.Category)
                .Where(t => t.ApprovalStatus == ApprovalStatus.Approved);

            if (!string.IsNullOrWhiteSpace(searchTerm))
                query = query.Where(t =>
                    t.Title.Contains(searchTerm) || t.Artist.Contains(searchTerm) ||
                    (t.Description != null && t.Description.Contains(searchTerm)));

            if (categoryId.HasValue)
                query = query.Where(t => t.CategoryId == categoryId.Value);

            return query.OrderByDescending(t => t.IsFeatured).ThenByDescending(t => t.CreatedAt);
        }

        private static async Task<PaginatedResult<T>> ToPaginatedAsync<T>(IQueryable<T> query, int page, int pageSize)
        {
            var totalCount = await query.CountAsync();
            var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

            return new PaginatedResult<T>
            {
                Items = items,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };
        }
    }
}
