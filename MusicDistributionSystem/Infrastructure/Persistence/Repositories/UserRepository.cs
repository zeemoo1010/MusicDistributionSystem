using Microsoft.EntityFrameworkCore;
using MusicDistributionSystem.Domain.Entities;
using MusicDistributionSystem.Application.Contracts.Repositories;

namespace MusicDistributionSystem.Infrastructure.Persistence.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly ApplicationDbContext _context;

        public UserRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public Task<User?> GetByEmailAsync(string email)
        {
            return _context.Users
                .Include(user => user.UserRoles)
                .ThenInclude(userRole => userRole.Role)
                .FirstOrDefaultAsync(user => user.Email == email);
        }

        public Task<User?> GetByIdAsync(Guid id)
        {
            return _context.Users.FirstOrDefaultAsync(user => user.Id == id);
        }

        public Task<User?> GetByIdWithRolesAsync(Guid id)
        {
            return _context.Users
                .Include(user => user.UserRoles)
                .ThenInclude(userRole => userRole.Role)
                .Include(user => user.UploadedTracks)
                .FirstOrDefaultAsync(user => user.Id == id);
        }

        public async Task<IReadOnlyCollection<User>> GetAllWithRolesAsync()
        {
            return await _context.Users
                .AsNoTracking()
                .Include(user => user.UserRoles)
                .ThenInclude(userRole => userRole.Role)
                .OrderBy(user => user.Username)
                .ToListAsync();
        }

        public async Task AddAsync(User user)
        {
            await _context.Users.AddAsync(user);
        }

        public async Task AddUserRoleAsync(UserRole userRole)
        {
            await _context.UserRoles.AddAsync(userRole);
        }

        public async Task RemoveUserRolesAsync(Guid userId)
        {
            var roles = await _context.UserRoles
                .Where(userRole => userRole.UserId == userId)
                .ToListAsync();

            _context.UserRoles.RemoveRange(roles);
        }

        public Task SaveChangesAsync()
        {
            return _context.SaveChangesAsync();
        }
    }
}
