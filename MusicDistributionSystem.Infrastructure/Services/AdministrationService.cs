using MusicDistributionSystem.Application.Contracts.Services;
using MusicDistributionSystem.Application.DTOs.Account;
using MusicDistributionSystem.Application.DTOs.Admin;
using MusicDistributionSystem.Domain.Constants;
using MusicDistributionSystem.Domain.Contracts.Interface;
using MusicDistributionSystem.Domain.Entities;

namespace MusicDistributionSystem.Application.Services
{
    public class AdministrationService : IAdministrationService
    {
        private readonly IUserRepository _userRepository;
        private readonly IRoleRepository _roleRepository;
        private readonly IMusicRepository _musicRepository;
        private readonly ICategoryRepository _categoryRepository;

        public AdministrationService(
            IUserRepository userRepository,
            IRoleRepository roleRepository,
            IMusicRepository musicRepository,
            ICategoryRepository categoryRepository)
        {
            _userRepository = userRepository;
            _roleRepository = roleRepository;
            _musicRepository = musicRepository;
            _categoryRepository = categoryRepository;
        }

        public async Task<AdminDashboardDto> GetDashboardAsync()
        {
            var users = await _userRepository.GetAllWithRolesAsync();
            var roles = await _roleRepository.GetAllAsync();
            var pendingTracks = await _musicRepository.GetPendingTracksAsync();
            var categories = await _categoryRepository.GetAllAsync();

            return new AdminDashboardDto
            {
                TotalUsers = users.Count,
                TotalTracks = await _musicRepository.CountAllAsync(),
                PendingTracks = pendingTracks.Count,
                ApprovedTracks = await _musicRepository.CountApprovedAsync(),
                RejectedTracks = await _musicRepository.CountRejectedAsync(),
                TotalCategories = categories.Count,
                AvailableRoles = roles.Select(role => role.Name).ToArray(),
                Categories = categories.Select(category => new AdminCategoryDto
                {
                    Id = category.Id,
                    Name = category.Name,
                    Description = category.Description,
                    LinkedTrackCount = category.MusicTracks.Count
                }).ToArray(),
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

        public async Task<OperationResultDto> UpdateUserRolesAsync(UpdateUserRolesRequestDto request, bool actorIsSuperAdmin)
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
                    Roles = new[] { RoleNames.User }
                };
            }

            if (!actorIsSuperAdmin && request.Roles.Contains(RoleNames.SuperAdmin, StringComparer.OrdinalIgnoreCase))
            {
                return new OperationResultDto
                {
                    ErrorMessage = "Only a super admin can assign the SuperAdmin role."
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

                await _userRepository.AddUserRoleAsync(new UserRole
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

        public async Task<OperationResultDto> CreateCategoryAsync(CreateCategoryRequestDto request)
        {
            var existingCategory = await _categoryRepository.GetByNameAsync(request.Name.Trim());
            if (existingCategory is not null)
            {
                return new OperationResultDto { ErrorMessage = "A category with this name already exists." };
            }

            await _categoryRepository.AddAsync(new Category
            {
                Name = request.Name.Trim(),
                Description = request.Description?.Trim()
            });
            await _categoryRepository.SaveChangesAsync();

            return new OperationResultDto { Succeeded = true };
        }

        public async Task<OperationResultDto> UpdateCategoryAsync(UpdateCategoryRequestDto request)
        {
            var category = await _categoryRepository.GetByIdAsync(request.Id);
            if (category is null)
            {
                return new OperationResultDto { ErrorMessage = "Category not found." };
            }

            var duplicateCategory = await _categoryRepository.GetByNameAsync(request.Name.Trim());
            if (duplicateCategory is not null && duplicateCategory.Id != request.Id)
            {
                return new OperationResultDto { ErrorMessage = "Another category already uses this name." };
            }

            category.Name = request.Name.Trim();
            category.Description = request.Description?.Trim();
            await _categoryRepository.SaveChangesAsync();

            return new OperationResultDto { Succeeded = true };
        }

        public async Task<OperationResultDto> DeleteCategoryAsync(Guid categoryId)
        {
            var category = await _categoryRepository.GetByIdAsync(categoryId);
            if (category is null)
            {
                return new OperationResultDto { ErrorMessage = "Category not found." };
            }

            var linkedTrackCount = await _categoryRepository.GetLinkedTrackCountAsync(categoryId);
            if (linkedTrackCount > 0)
            {
                return new OperationResultDto { ErrorMessage = "You cannot delete a category that still has music linked to it." };
            }

            _categoryRepository.Remove(category);
            await _categoryRepository.SaveChangesAsync();

            return new OperationResultDto { Succeeded = true };
        }
    }
}

