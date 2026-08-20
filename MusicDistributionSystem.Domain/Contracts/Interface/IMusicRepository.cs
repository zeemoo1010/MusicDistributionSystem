using MusicDistributionSystem.Domain.Common;
using MusicDistributionSystem.Domain.Entities;

namespace MusicDistributionSystem.Domain.Contracts.Interface
{
    public interface IMusicRepository
    {
        Task<IReadOnlyCollection<MusicTrack>> GetApprovedTracksAsync(string? searchTerm, Guid? categoryId);
        Task<PaginatedResult<MusicTrack>> GetApprovedTracksPagedAsync(string? searchTerm, Guid? categoryId, int page, int pageSize);
        Task<IReadOnlyCollection<MusicTrack>> GetLatestApprovedTracksAsync(int take);
        Task<IReadOnlyCollection<MusicTrack>> GetTrendingTracksAsync(int take);
        Task<IReadOnlyCollection<MusicTrack>> GetTopChartTracksAsync(int take);
        Task<MusicTrack?> GetApprovedTrackByIdAsync(Guid id, bool asNoTracking = true);

        Task<MusicTrack?> GetApprovedTrackBySlugAsync(string slug, bool asNoTracking = true);
        Task<MusicTrack?> GetByIdAsync(Guid id, bool asNoTracking = true);
        Task<IReadOnlyCollection<MusicTrack>> GetPendingTracksAsync();
        Task<PaginatedResult<MusicTrack>> GetPendingTracksPagedAsync(int page, int pageSize);
        Task<IReadOnlyCollection<MusicTrack>> GetTracksByUploaderAsync(Guid uploaderUserId);
        Task<PaginatedResult<MusicTrack>> GetTracksByUploaderPagedAsync(Guid uploaderUserId, int page, int pageSize);
        Task<int> CountAllAsync();
        Task<int> CountApprovedAsync();
        Task<int> CountRejectedAsync();
        Task<int> CountPremiumApprovedAsync();
        Task<int> GetTotalDownloadsAsync();
        Task AddAsync(MusicTrack track);
        void Remove(MusicTrack track);
        Task AddDownloadRecordAsync(DownloadRecord downloadRecord);
        Task SaveChangesAsync();
    }
}
