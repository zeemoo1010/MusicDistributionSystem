using MusicDistributionSystem.DTOs.Account;
using MusicDistributionSystem.DTOs.Dashboard;

namespace MusicDistributionSystem.Services.Interfaces
{
    public interface IDashboardService
    {
        Task<DashboardIndexDto?> GetDashboardAsync(Guid userId);
        Task<OperationResultDto> DeleteOwnUploadAsync(Guid userId, Guid trackId, bool isAdmin);
    }
}
