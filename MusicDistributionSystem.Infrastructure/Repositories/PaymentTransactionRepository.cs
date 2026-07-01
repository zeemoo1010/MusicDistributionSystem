using Microsoft.EntityFrameworkCore;
using MusicDistributionSystem.Domain.Contracts.Interface;
using MusicDistributionSystem.Domain.Entities;

namespace MusicDistributionSystem.Infrastructure.EntityFrameworkCore.Repositories
{
    public class PaymentTransactionRepository : IPaymentTransactionRepository
    {
        private readonly ApplicationDbContext _context;

        public PaymentTransactionRepository(ApplicationDbContext context) => _context = context;

        public Task<PaymentTransaction?> GetByReferenceAsync(string reference)
        {
            return _context.PaymentTransactions.FirstOrDefaultAsync(t => t.Reference == reference);
        }

        public async Task<IReadOnlyCollection<PaymentTransaction>> GetByUserAsync(Guid userId)
        {
            return await _context.PaymentTransactions
                .AsNoTracking()
                .Where(t => t.UserId == userId)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();
        }

        public async Task AddAsync(PaymentTransaction transaction) => await _context.PaymentTransactions.AddAsync(transaction);
        public Task SaveChangesAsync() => _context.SaveChangesAsync();
    }
}