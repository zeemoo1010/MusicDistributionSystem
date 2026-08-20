using MusicDistributionSystem.Domain.Common;
using MusicDistributionSystem.Domain.Entities;

namespace MusicDistributionSystem.Domain.Contracts.Interface
{
    public interface IImageAssetRepository
    {
        Task<IReadOnlyCollection<ImageAsset>> GetApprovedImagesAsync(string? searchTerm, Guid? categoryId);
        Task<PaginatedResult<ImageAsset>> GetApprovedImagesPagedAsync(string? searchTerm, Guid? categoryId, int page, int pageSize);
        Task<ImageAsset?> GetByIdAsync(Guid id, bool asNoTracking = true);
        Task<ImageAsset?> GetBySlugAsync(string slug, bool asNoTracking = true);
        Task<IReadOnlyCollection<ImageAsset>> GetFeaturedImagesAsync(int take);
        Task<int> CountApprovedAsync();
        Task AddAsync(ImageAsset image);
        void Remove(ImageAsset image);
        Task SaveChangesAsync();
    }
}
