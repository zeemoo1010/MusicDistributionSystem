using MusicDistributionSystem.Models;

namespace MusicDistributionSystem.Repositories.Interfaces
{
    public interface IMusicRepository
    {
        Task<IReadOnlyCollection<MusicTrack>> GetApprovedTracksAsync(string? searchTerm, Guid? categoryId);
        Task<IReadOnlyCollection<MusicTrack>> GetLatestApprovedTracksAsync(int take);
        Task<MusicTrack?> GetApprovedTrackByIdAsync(Guid id, bool asNoTracking = true);
        Task<int> CountApprovedAsync();
        Task<int> CountPremiumApprovedAsync();
        Task<int> GetTotalDownloadsAsync();
        Task AddAsync(MusicTrack track);
        Task AddDownloadRecordAsync(DownloadRecord downloadRecord);
        Task SaveChangesAsync();
    }
}
