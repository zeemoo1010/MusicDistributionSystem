using MusicDistributionSystem.Domain.Entities;

namespace MusicDistributionSystem.Domain.Contracts.Interface
{
    public interface INotificationRepository
    {
        Task<IReadOnlyCollection<Notification>> GetByUserAsync(Guid userId, int take);
        Task<int> GetUnreadCountAsync(Guid userId);
        Task AddAsync(Notification notification);
        Task MarkAsReadAsync(Guid notificationId);
        Task MarkAllAsReadAsync(Guid userId);
        Task SaveChangesAsync();
    }
}