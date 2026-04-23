using MusicDistributionSystem.DTOs.Account;
using MusicDistributionSystem.DTOs.Moderation;

namespace MusicDistributionSystem.Services.Interfaces
{
    public interface IModerationService
    {
        Task<ModerationDashboardDto> GetDashboardAsync();
        Task<OperationResultDto> ApproveTrackAsync(Guid trackId);
        Task<OperationResultDto> RejectTrackAsync(Guid trackId);
    }
}
