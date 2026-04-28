using MusicDistributionSystem.Domain.Entities;

namespace MusicDistributionSystem.Application.Contracts.Repositories
{
    public interface IRoleRepository
    {
        Task<Role?> GetByNameAsync(string roleName);
        Task<IReadOnlyCollection<Role>> GetAllAsync();
        Task AddAsync(Role role);
        Task SaveChangesAsync();
    }
}

