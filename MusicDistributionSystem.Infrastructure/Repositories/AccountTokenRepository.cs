using Microsoft.EntityFrameworkCore;
using MusicDistributionSystem.Domain.Contracts.Interface;
using MusicDistributionSystem.Domain.Entities;
using MusicDistributionSystem.Domain.Enums;

namespace MusicDistributionSystem.Infrastructure.EntityFrameworkCore.Repositories
{
    public class AccountTokenRepository : IAccountTokenRepository
    {
        private readonly ApplicationDbContext _context;

        public AccountTokenRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(AccountToken token)
        {
            await _context.AccountTokens.AddAsync(token);
        }

        public Task<AccountToken?> GetLatestActiveTokenAsync(Guid userId, AccountTokenType type)
        {
            return _context.AccountTokens
                .Where(token =>
                    token.UserId == userId &&
                    token.Type == type &&
                    token.ConsumedAtUtc == null &&
                    token.ExpiresAtUtc > DateTime.UtcNow)
                .OrderByDescending(token => token.CreatedAtUtc)
                .FirstOrDefaultAsync();
        }

        public async Task InvalidateActiveTokensAsync(Guid userId, AccountTokenType type)
        {
            var activeTokens = await _context.AccountTokens
                .Where(token =>
                    token.UserId == userId &&
                    token.Type == type &&
                    token.ConsumedAtUtc == null)
                .ToListAsync();

            foreach (var token in activeTokens)
            {
                token.ConsumedAtUtc = DateTime.UtcNow;
            }
        }

        public Task SaveChangesAsync()
        {
            return _context.SaveChangesAsync();
        }
    }
}

