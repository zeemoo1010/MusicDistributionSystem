using Microsoft.EntityFrameworkCore;
using MusicDistributionSystem.Domain.Enums;
using MusicDistributionSystem.Domain.Entities;
using MusicDistributionSystem.Application.Contracts.Repositories;

namespace MusicDistributionSystem.Infrastructure.Persistence.Repositories
{
    public class MusicRepository : IMusicRepository
    {
        private readonly ApplicationDbContext _context;

        public MusicRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IReadOnlyCollection<MusicTrack>> GetApprovedTracksAsync(string? searchTerm, Guid? categoryId)
        {
            var query = _context.MusicTracks
                .AsNoTracking()
                .Include(track => track.Category)
                .Where(track => track.ApprovalStatus == ApprovalStatus.Approved);

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                query = query.Where(track =>
                    track.Title.Contains(searchTerm) ||
                    track.Artist.Contains(searchTerm) ||
                    (track.Description != null && track.Description.Contains(searchTerm)));
            }

            if (categoryId.HasValue)
            {
                query = query.Where(track => track.CategoryId == categoryId.Value);
            }

            return await query
                .OrderByDescending(track => track.IsFeatured)
                .ThenByDescending(track => track.CreatedAt)
                .ToListAsync();
        }

        public async Task<IReadOnlyCollection<MusicTrack>> GetLatestApprovedTracksAsync(int take)
        {
            return await _context.MusicTracks
                .AsNoTracking()
                .Include(track => track.Category)
                .Where(track => track.ApprovalStatus == ApprovalStatus.Approved)
                .OrderByDescending(track => track.IsFeatured)
                .ThenByDescending(track => track.CreatedAt)
                .Take(take)
                .ToListAsync();
        }

        public async Task<MusicTrack?> GetApprovedTrackByIdAsync(Guid id, bool asNoTracking = true)
        {
            var query = _context.MusicTracks
                .Include(track => track.Category)
                .Where(track => track.Id == id && track.ApprovalStatus == ApprovalStatus.Approved);

            if (asNoTracking)
            {
                query = query.AsNoTracking();
            }

            return await query.FirstOrDefaultAsync();
        }

        public async Task<MusicTrack?> GetByIdAsync(Guid id, bool asNoTracking = true)
        {
            var query = _context.MusicTracks
                .Include(track => track.Category)
                .Include(track => track.UploadedByUser)
                .Where(track => track.Id == id);

            if (asNoTracking)
            {
                query = query.AsNoTracking();
            }

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

        public async Task<IReadOnlyCollection<MusicTrack>> GetTracksByUploaderAsync(Guid uploaderUserId)
        {
            return await _context.MusicTracks
                .AsNoTracking()
                .Include(track => track.Category)
                .Where(track => track.UploadedByUserId == uploaderUserId)
                .OrderByDescending(track => track.CreatedAt)
                .ToListAsync();
        }

        public Task<int> CountApprovedAsync()
        {
            return _context.MusicTracks.CountAsync(track => track.ApprovalStatus == ApprovalStatus.Approved);
        }

        public Task<int> CountPremiumApprovedAsync()
        {
            return _context.MusicTracks.CountAsync(track =>
                track.ApprovalStatus == ApprovalStatus.Approved &&
                track.AccessLevel != ContentAccessLevel.Free);
        }

        public async Task<int> GetTotalDownloadsAsync()
        {
            return await _context.MusicTracks.SumAsync(track => (int?)track.DownloadCount) ?? 0;
        }

        public async Task AddAsync(MusicTrack track)
        {
            await _context.MusicTracks.AddAsync(track);
        }

        public void Remove(MusicTrack track)
        {
            _context.MusicTracks.Remove(track);
        }

        public async Task AddDownloadRecordAsync(DownloadRecord downloadRecord)
        {
            await _context.DownloadRecords.AddAsync(downloadRecord);
        }

        public Task SaveChangesAsync()
        {
            return _context.SaveChangesAsync();
        }
    }
}
