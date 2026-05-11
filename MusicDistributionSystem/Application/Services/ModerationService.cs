using MusicDistributionSystem.Application.DTOs.Account;
using MusicDistributionSystem.Application.DTOs.Moderation;
using MusicDistributionSystem.Domain.Enums;
using MusicDistributionSystem.Application.Contracts.Repositories;
using MusicDistributionSystem.Application.Contracts.Services;

namespace MusicDistributionSystem.Application.Services
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
                    Description = track.Description,
                    CoverImagePath = track.CoverImagePath,
                    CreatedAt = track.CreatedAt,
                    AccessLevel = track.AccessLevel
                }).ToArray()
            };
        }

        public async Task<OperationResultDto> ApproveTrackAsync(Guid trackId, Guid reviewerUserId)
        {
            var track = await _musicRepository.GetByIdAsync(trackId, asNoTracking: false);
            if (track is null)
            {
                return new OperationResultDto { ErrorMessage = "Track not found." };
            }

            track.ApprovalStatus = ApprovalStatus.Approved;
            track.RejectionReason = null;
            track.ReviewedByUserId = reviewerUserId;
            track.ReviewedAtUtc = DateTime.UtcNow;
            await _musicRepository.SaveChangesAsync();

            return new OperationResultDto { Succeeded = true };
        }

        public async Task<OperationResultDto> RejectTrackAsync(RejectTrackRequestDto request, Guid reviewerUserId)
        {
            var track = await _musicRepository.GetByIdAsync(request.TrackId, asNoTracking: false);
            if (track is null)
            {
                return new OperationResultDto { ErrorMessage = "Track not found." };
            }

            track.ApprovalStatus = ApprovalStatus.Rejected;
            track.RejectionReason = request.Reason.Trim();
            track.ReviewedByUserId = reviewerUserId;
            track.ReviewedAtUtc = DateTime.UtcNow;
            await _musicRepository.SaveChangesAsync();

            return new OperationResultDto { Succeeded = true };
        }
    }
}

