using MusicDistributionSystem.Domain.Entities;

namespace MusicDistributionSystem.Domain.Contracts.Interface
{
    public interface IPaymentTransactionRepository
    {
        Task<PaymentTransaction?> GetByReferenceAsync(string reference);
        Task<IReadOnlyCollection<PaymentTransaction>> GetByUserAsync(Guid userId);
        Task AddAsync(PaymentTransaction transaction);
        Task SaveChangesAsync();
    }
}