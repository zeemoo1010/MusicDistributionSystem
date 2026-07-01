using Microsoft.EntityFrameworkCore;
using MusicDistributionSystem.Domain.Contracts.Interface;
using MusicDistributionSystem.Domain.Entities;

namespace MusicDistributionSystem.Infrastructure.EntityFrameworkCore.Repositories
{
    public class LikeRepository : ILikeRepository
    {
        private readonly ApplicationDbContext _context;

        public LikeRepository(ApplicationDbContext context) => _context = context;

        public Task<bool> IsLikedAsync(Guid userId, Guid mediaAssetId)
        {
            return _context.Likes.AnyAsync(l => l.UserId == userId && l.MediaAssetId == mediaAssetId);
        }

        public Task<int> CountByMediaAssetAsync(Guid mediaAssetId)
        {
            return _context.Likes.CountAsync(l => l.MediaAssetId == mediaAssetId);
        }

        public async Task AddAsync(Like like) => await _context.Likes.AddAsync(like);
        public void Remove(Like like) => _context.Likes.Remove(like);
        public Task SaveChangesAsync() => _context.SaveChangesAsync();
    }
}