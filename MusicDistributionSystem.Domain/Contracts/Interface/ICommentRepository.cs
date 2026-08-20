using MusicDistributionSystem.Domain.Entities;

namespace MusicDistributionSystem.Domain.Contracts.Interface
{
    public interface ICommentRepository
    {
        Task<IReadOnlyCollection<Comment>> GetByTrackAsync(Guid trackId);
        Task<Comment?> GetByIdAsync(Guid id);
        Task AddAsync(Comment comment);
        void Remove(Comment comment);
        Task SaveChangesAsync();
    }
}