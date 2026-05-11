using MusicDistributionSystem.Application.DTOs.Account;
using MusicDistributionSystem.Application.DTOs.Moderation;

namespace MusicDistributionSystem.Application.Contracts.Services
{
    public interface IModerationService
    {
        Task<ModerationDashboardDto> GetDashboardAsync();
        Task<OperationResultDto> ApproveTrackAsync(Guid trackId, Guid reviewerUserId);
        Task<OperationResultDto> RejectTrackAsync(RejectTrackRequestDto request, Guid reviewerUserId);
    }
}

