using MusicDistributionSystem.Domain.Enums;
using MusicDistributionSystem.Domain.Entities;

namespace MusicDistributionSystem.Domain.Contracts.Interface
{
    public interface IAccountTokenRepository
    {
        Task AddAsync(AccountToken token);
        Task<AccountToken?> GetLatestActiveTokenAsync(Guid userId, AccountTokenType type);
        Task InvalidateActiveTokensAsync(Guid userId, AccountTokenType type);
        Task SaveChangesAsync();
    }
}

