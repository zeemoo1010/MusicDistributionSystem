using Microsoft.EntityFrameworkCore;
using MusicDistributionSystem.Domain.Contracts.Interface;
using MusicDistributionSystem.Domain.Entities;

namespace MusicDistributionSystem.Infrastructure.EntityFrameworkCore.Repositories
{
    public class UserSubscriptionRepository : IUserSubscriptionRepository
    {
        private readonly ApplicationDbContext _context;

        public UserSubscriptionRepository(ApplicationDbContext context) => _context = context;

        public Task<UserSubscription?> GetActiveByUserAsync(Guid userId)
        {
            return _context.UserSubscriptions
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.UserId == userId && s.IsActive);
        }

        public async Task AddAsync(UserSubscription subscription) => await _context.UserSubscriptions.AddAsync(subscription);
        public Task SaveChangesAsync() => _context.SaveChangesAsync();
    }
}