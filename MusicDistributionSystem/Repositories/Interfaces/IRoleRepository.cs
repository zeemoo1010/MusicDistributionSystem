using MusicDistributionSystem.Models;

namespace MusicDistributionSystem.Repositories.Interfaces
{
    public interface IRoleRepository
    {
        Task<Role?> GetByNameAsync(string roleName);
        Task<IReadOnlyCollection<Role>> GetAllAsync();
        Task AddAsync(Role role);
        Task SaveChangesAsync();
    }
}
