using MusicDistributionSystem.DTOs.Account;
using MusicDistributionSystem.DTOs.Moderation;
using MusicDistributionSystem.Enums;
using MusicDistributionSystem.Repositories.Interfaces;
using MusicDistributionSystem.Services.Interfaces;

namespace MusicDistributionSystem.Services
{
    public class ModerationService : IModerationService
    {
        private readonly IMusicRepository _musicRepository;

        public ModerationService(IMusicRepository musicRepository)
        {
            _musicRepository = musicRepository;
        }

        public async Task<ModerationDashboardDto> GetDashboardAsync()
        {
            var pendingTracks = await _musicRepository.GetPendingTracksAsync();

            return new ModerationDashboardDto
            {
                PendingTracks = pendingTracks.Select(track => new ModerationTrackDto
                {
                    Id = track.Id,
                    Title = track.Title,
                    Artist = track.Artist,
                    UploadedByName = track.UploadedByName,
                    UploadedByEmail = track.UploadedByEmail,
                    CategoryName = track.Category?.Name,
                    CreatedAt = track.CreatedAt,
                    AccessLevel = track.AccessLevel
                }).ToArray()
            };
        }

        public async Task<OperationResultDto> ApproveTrackAsync(Guid trackId)
        {
            var track = await _musicRepository.GetByIdAsync(trackId, asNoTracking: false);
            if (track is null)
            {
                return new OperationResultDto { ErrorMessage = "Track not found." };
            }

            track.ApprovalStatus = ApprovalStatus.Approved;
            await _musicRepository.SaveChangesAsync();

            return new OperationResultDto { Succeeded = true };
        }

        public async Task<OperationResultDto> RejectTrackAsync(Guid trackId)
        {
            var track = await _musicRepository.GetByIdAsync(trackId, asNoTracking: false);
            if (track is null)
            {
                return new OperationResultDto { ErrorMessage = "Track not found." };
            }

            track.ApprovalStatus = ApprovalStatus.Rejected;
            await _musicRepository.SaveChangesAsync();

            return new OperationResultDto { Succeeded = true };
        }
    }
}
