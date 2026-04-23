using MusicDistributionSystem.Constants;
using MusicDistributionSystem.DTOs.Account;
using MusicDistributionSystem.DTOs.Admin;
using MusicDistributionSystem.Repositories.Interfaces;
using MusicDistributionSystem.Services.Interfaces;

namespace MusicDistributionSystem.Services
{
    public class AdministrationService : IAdministrationService
    {
        private readonly IUserRepository _userRepository;
        private readonly IRoleRepository _roleRepository;
        private readonly IMusicRepository _musicRepository;

        public AdministrationService(
            IUserRepository userRepository,
            IRoleRepository roleRepository,
            IMusicRepository musicRepository)
        {
            _userRepository = userRepository;
            _roleRepository = roleRepository;
            _musicRepository = musicRepository;
        }

        public async Task<AdminDashboardDto> GetDashboardAsync()
        {
            var users = await _userRepository.GetAllWithRolesAsync();
            var roles = await _roleRepository.GetAllAsync();
            var pendingTracks = await _musicRepository.GetPendingTracksAsync();

            return new AdminDashboardDto
            {
                TotalUsers = users.Count,
                PendingTracks = pendingTracks.Count,
                ApprovedTracks = await _musicRepository.CountApprovedAsync(),
                AvailableRoles = roles.Select(role => role.Name).ToArray(),
                Users = users.Select(user => new AdminUserDto
                {
                    UserId = user.Id,
                    Username = user.Username,
                    Email = user.Email,
                    IsActive = user.IsActive,
                    IsEmailVerified = user.IsEmailVerified,
                    Roles = user.UserRoles
                        .Select(userRole => userRole.Role?.Name ?? string.Empty)
                        .Where(roleName => !string.IsNullOrWhiteSpace(roleName))
                        .ToArray()
                }).ToArray()
            };
        }

        public async Task<OperationResultDto> UpdateUserRolesAsync(UpdateUserRolesRequestDto request)
        {
            var user = await _userRepository.GetByIdWithRolesAsync(request.UserId);
            if (user is null)
            {
                return new OperationResultDto
                {
                    ErrorMessage = "User not found."
                };
            }

            if (!request.Roles.Any())
            {
                request = new UpdateUserRolesRequestDto
                {
                    UserId = request.UserId,
                    Roles = new[] { RoleNames.RegisteredUser }
                };
            }

            await _userRepository.RemoveUserRolesAsync(user.Id);

            foreach (var roleName in request.Roles.Distinct(StringComparer.OrdinalIgnoreCase))
            {
                var role = await _roleRepository.GetByNameAsync(roleName);
                if (role is null)
                {
                    continue;
                }

                await _userRepository.AddUserRoleAsync(new Models.UserRole
                {
                    UserId = user.Id,
                    RoleId = role.Id
                });
            }

            await _userRepository.SaveChangesAsync();

            return new OperationResultDto
            {
                Succeeded = true
            };
        }
    }
}
