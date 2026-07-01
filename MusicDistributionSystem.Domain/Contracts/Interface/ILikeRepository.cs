using MusicDistributionSystem.Domain.Entities;

namespace MusicDistributionSystem.Domain.Contracts.Interface
{
    public interface ILikeRepository
    {
        Task<bool> IsLikedAsync(Guid userId, Guid mediaAssetId);
        Task<int> CountByMediaAssetAsync(Guid mediaAssetId);
        Task AddAsync(Like like);
        void Remove(Like like);
        Task SaveChangesAsync();
    }
}