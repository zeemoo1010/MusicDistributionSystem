using MusicDistributionSystem.DTOs.Admin;
using MusicDistributionSystem.DTOs.Account;

namespace MusicDistributionSystem.Services.Interfaces
{
    public interface IAdministrationService
    {
        Task<AdminDashboardDto> GetDashboardAsync();
        Task<OperationResultDto> UpdateUserRolesAsync(UpdateUserRolesRequestDto request);
    }
}
