using MusicDistributionSystem.Domain.Entities;

namespace MusicDistributionSystem.Domain.Contracts.Interface
{
    public interface ICategoryRepository
    {
        Task<IReadOnlyCollection<Category>> GetAllAsync();
        Task<Category?> GetByIdAsync(Guid id);
        Task<Category?> GetByIdWithTracksAsync(Guid id);
        Task<Category?> GetByNameAsync(string name);
        Task AddAsync(Category category);
        void Remove(Category category);
        Task<int> GetLinkedTrackCountAsync(Guid categoryId);
        Task SaveChangesAsync();
    }
}
