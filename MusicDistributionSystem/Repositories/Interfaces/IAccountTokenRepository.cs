using MusicDistributionSystem.Enums;
using MusicDistributionSystem.Models;

namespace MusicDistributionSystem.Repositories.Interfaces
{
    public interface IAccountTokenRepository
    {
        Task AddAsync(AccountToken token);
        Task<AccountToken?> GetLatestActiveTokenAsync(Guid userId, AccountTokenType type);
        Task InvalidateActiveTokensAsync(Guid userId, AccountTokenType type);
        Task SaveChangesAsync();
    }
}
