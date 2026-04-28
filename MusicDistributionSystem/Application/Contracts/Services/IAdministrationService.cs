using MusicDistributionSystem.Application.DTOs.Admin;
using MusicDistributionSystem.Application.DTOs.Account;

namespace MusicDistributionSystem.Application.Contracts.Services
{
    public interface IAdministrationService
    {
        Task<AdminDashboardDto> GetDashboardAsync();
        Task<OperationResultDto> UpdateUserRolesAsync(UpdateUserRolesRequestDto request);
    }
}

