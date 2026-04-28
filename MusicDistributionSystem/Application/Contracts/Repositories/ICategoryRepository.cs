using MusicDistributionSystem.Domain.Entities;

namespace MusicDistributionSystem.Application.Contracts.Repositories
{
    public interface ICategoryRepository
    {
        Task<IReadOnlyCollection<Category>> GetAllAsync();
    }
}

