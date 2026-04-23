using MusicDistributionSystem.Models;

namespace MusicDistributionSystem.Repositories.Interfaces
{
    public interface IUserRepository
    {
        Task<User?> GetByEmailAsync(string email);
        Task<User?> GetByIdAsync(Guid id);
        Task<User?> GetByIdWithRolesAsync(Guid id);
        Task<IReadOnlyCollection<User>> GetAllWithRolesAsync();
        Task AddAsync(User user);
        Task AddUserRoleAsync(UserRole userRole);
        Task RemoveUserRolesAsync(Guid userId);
        Task SaveChangesAsync();
    }
}
