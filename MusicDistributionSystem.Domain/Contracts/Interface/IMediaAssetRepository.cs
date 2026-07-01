using MusicDistributionSystem.Domain.Entities;
using MusicDistributionSystem.Domain.Enums;

namespace MusicDistributionSystem.Domain.Contracts.Interface
{
    public interface IMediaAssetRepository
    {
        Task<IReadOnlyCollection<MediaAsset>> GetApprovedAsync(string? searchTerm, Guid? categoryId, MediaType? type, int page, int pageSize);
        Task<IReadOnlyCollection<MediaAsset>> GetLatestAsync(int take);
        Task<MediaAsset?> GetApprovedByIdAsync(Guid id, bool asNoTracking = true);
        Task<MediaAsset?> GetByIdAsync(Guid id, bool asNoTracking = true);
        Task<IReadOnlyCollection<MediaAsset>> GetPendingAsync();
        Task<IReadOnlyCollection<MediaAsset>> GetByUploaderAsync(Guid uploaderUserId);
        Task<int> CountByStatusAsync(ApprovalStatus status);
        Task<int> CountByTypeAsync(MediaType type);
        Task<int> GetTotalDownloadsAsync();
        Task AddAsync(MediaAsset asset);
        void Remove(MediaAsset asset);
        Task SaveChangesAsync();
    }
}