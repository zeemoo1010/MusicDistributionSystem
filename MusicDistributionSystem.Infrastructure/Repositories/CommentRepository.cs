using Microsoft.EntityFrameworkCore;
using MusicDistributionSystem.Domain.Contracts.Interface;
using MusicDistributionSystem.Domain.Entities;

namespace MusicDistributionSystem.Infrastructure.EntityFrameworkCore.Repositories
{
    public class CommentRepository : ICommentRepository
    {
        private readonly ApplicationDbContext _context;

        public CommentRepository(ApplicationDbContext context) => _context = context;

        public async Task<IReadOnlyCollection<Comment>> GetByTrackAsync(Guid trackId)
        {
            return await _context.Comments
                .AsNoTracking()
                .Include(c => c.User)
                .Include(c => c.Replies)
                .Where(c => c.MusicTrackId == trackId && c.ParentCommentId == null)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();
        }

        public Task<Comment?> GetByIdAsync(Guid id)
        {
            return _context.Comments.FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task AddAsync(Comment comment) => await _context.Comments.AddAsync(comment);
        public void Remove(Comment comment) => _context.Comments.Remove(comment);
        public Task SaveChangesAsync() => _context.SaveChangesAsync();
    }
}