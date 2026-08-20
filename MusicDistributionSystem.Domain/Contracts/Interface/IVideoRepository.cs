using MusicDistributionSystem.Domain.Common;
using MusicDistributionSystem.Domain.Entities;
using MusicDistributionSystem.Domain.Enums;

namespace MusicDistributionSystem.Domain.Contracts.Interface
{
    public interface IVideoRepository
    {
        Task<IReadOnlyCollection<Video>> GetApprovedVideosAsync(string? searchTerm, Guid? categoryId);
        Task<PaginatedResult<Video>> GetApprovedVideosPagedAsync(string? searchTerm, Guid? categoryId, int page, int pageSize);
        Task<IReadOnlyCollection<Video>> GetFeaturedVideosAsync(int take);
        Task<IReadOnlyCollection<Video>> GetLatestApprovedVideosAsync(int take);
        Task<Video?> GetApprovedVideoByIdAsync(Guid id, bool asNoTracking = true);
        Task<Video?> GetApprovedVideoBySlugAsync(string slug, bool asNoTracking = true);
        Task<Video?> GetByIdAsync(Guid id, bool asNoTracking = true);
        Task<IReadOnlyCollection<Video>> GetPendingVideosAsync();
        Task<IReadOnlyCollection<Video>> GetVideosByUploaderAsync(Guid uploaderUserId);
        Task<int> CountApprovedAsync();
        Task<int> CountPendingAsync();
        Task AddAsync(Video video);
        void Remove(Video video);
        Task SaveChangesAsync();
    }
}
