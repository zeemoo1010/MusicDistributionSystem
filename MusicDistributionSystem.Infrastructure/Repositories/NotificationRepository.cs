using Microsoft.EntityFrameworkCore;
using MusicDistributionSystem.Domain.Contracts.Interface;
using MusicDistributionSystem.Domain.Entities;

namespace MusicDistributionSystem.Infrastructure.EntityFrameworkCore.Repositories
{
    public class NotificationRepository : INotificationRepository
    {
        private readonly ApplicationDbContext _context;

        public NotificationRepository(ApplicationDbContext context) => _context = context;

        public async Task<IReadOnlyCollection<Notification>> GetByUserAsync(Guid userId, int take)
        {
            return await _context.Notifications
                .AsNoTracking()
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.CreatedAt)
                .Take(take)
                .ToListAsync();
        }

        public Task<int> GetUnreadCountAsync(Guid userId)
        {
            return _context.Notifications.CountAsync(n => n.UserId == userId && !n.IsRead);
        }

        public async Task AddAsync(Notification notification) => await _context.Notifications.AddAsync(notification);

        public async Task MarkAsReadAsync(Guid notificationId)
        {
            var notification = await _context.Notifications.FindAsync(notificationId);
            if (notification is not null)
                notification.IsRead = true;
        }

        public async Task MarkAllAsReadAsync(Guid userId)
        {
            await _context.Notifications
                .Where(n => n.UserId == userId && !n.IsRead)
                .ExecuteUpdateAsync(setters => setters.SetProperty(n => n.IsRead, true));
        }

        public Task SaveChangesAsync() => _context.SaveChangesAsync();
    }
}