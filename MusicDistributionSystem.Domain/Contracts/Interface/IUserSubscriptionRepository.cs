using MusicDistributionSystem.Domain.Entities;

namespace MusicDistributionSystem.Domain.Contracts.Interface
{
    public interface IUserSubscriptionRepository
    {
        Task<UserSubscription?> GetActiveByUserAsync(Guid userId);
        Task AddAsync(UserSubscription subscription);
        Task SaveChangesAsync();
    }
}