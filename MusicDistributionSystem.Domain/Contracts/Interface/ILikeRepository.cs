using MusicDistributionSystem.Domain.Entities;

namespace MusicDistributionSystem.Domain.Contracts.Interface
{
    public interface ILikeRepository
    {
        Task<bool> IsLikedAsync(Guid userId, Guid trackId);
        Task<int> CountByTrackAsync(Guid trackId);
        Task AddAsync(Like like);
        void Remove(Like like);
        Task SaveChangesAsync();
    }
}