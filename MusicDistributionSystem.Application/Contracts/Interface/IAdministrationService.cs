using MusicDistributionSystem.Application.DTOs.Admin;
using MusicDistributionSystem.Application.DTOs.Account;

namespace MusicDistributionSystem.Application.Contracts.Services
{
    public interface IAdministrationService
    {
        Task<AdminDashboardDto> GetDashboardAsync();
        Task<OperationResultDto> UpdateUserRolesAsync(UpdateUserRolesRequestDto request, bool actorIsSuperAdmin);
        Task<OperationResultDto> CreateCategoryAsync(CreateCategoryRequestDto request);
        Task<OperationResultDto> UpdateCategoryAsync(UpdateCategoryRequestDto request);
        Task<OperationResultDto> DeleteCategoryAsync(Guid categoryId);
    }
}

