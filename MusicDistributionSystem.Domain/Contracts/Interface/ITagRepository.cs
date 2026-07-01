using MusicDistributionSystem.Domain.Entities;

namespace MusicDistributionSystem.Domain.Contracts.Interface
{
    public interface ITagRepository
    {
        Task<IReadOnlyCollection<Tag>> GetAllAsync();
        Task<Tag?> GetByNameAsync(string name);
        Task AddAsync(Tag tag);
        Task SaveChangesAsync();
    }
}