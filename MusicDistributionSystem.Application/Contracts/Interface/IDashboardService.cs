using MusicDistributionSystem.Application.DTOs.Account;
using MusicDistributionSystem.Application.DTOs.Dashboard;

namespace MusicDistributionSystem.Application.Contracts.Services
{
    public interface IDashboardService
    {
        Task<DashboardIndexDto?> GetDashboardAsync(Guid userId);
        Task<OperationResultDto> DeleteOwnUploadAsync(Guid userId, Guid trackId, bool isAdmin);
    }
}

