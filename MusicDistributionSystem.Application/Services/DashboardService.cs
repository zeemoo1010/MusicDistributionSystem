using Microsoft.AspNetCore.Hosting;
using MusicDistributionSystem.Application.Contracts.Services;
using MusicDistributionSystem.Application.DTOs.Account;
using MusicDistributionSystem.Application.DTOs.Dashboard;
using MusicDistributionSystem.Domain.Contracts.Interface;

namespace MusicDistributionSystem.Application.Services
{
    public class DashboardService : IDashboardService
    {
        private readonly IUserRepository _userRepository;
        private readonly IMusicRepository _musicRepository;
        private readonly IWebHostEnvironment _environment;

        public DashboardService(
            IUserRepository userRepository,
            IMusicRepository musicRepository,
            IWebHostEnvironment environment)
        {
            _userRepository = userRepository;
            _musicRepository = musicRepository;
            _environment = environment;
        }

        public async Task<DashboardIndexDto?> GetDashboardAsync(Guid userId)
        {
            var user = await _userRepository.GetByIdWithRolesAsync(userId);
            if (user is null)
            {
                return null;
            }

            var uploads = await _musicRepository.GetTracksByUploaderAsync(userId);

            return new DashboardIndexDto
            {
                Username = user.Username,
                Email = user.Email,
                Roles = user.UserRoles
                    .Select(userRole => userRole.Role?.Name ?? string.Empty)
                    .Where(roleName => !string.IsNullOrWhiteSpace(roleName))
                    .ToArray(),
                Uploads = uploads.Select(track => new DashboardTrackDto
                {
                    Id = track.Id,
                    Title = track.Title,
                    Artist = track.Artist,
                    CategoryName = track.Category?.Name,
                    ApprovalStatus = track.ApprovalStatus,
                    AccessLevel = track.AccessLevel,
                    RejectionReason = track.RejectionReason,
                    ReviewedAtUtc = track.ReviewedAtUtc,
                    CreatedAt = track.CreatedAt
                }).ToArray()
            };
        }

        public async Task<OperationResultDto> DeleteOwnUploadAsync(Guid userId, Guid trackId, bool isAdmin)
        {
            var track = await _musicRepository.GetByIdAsync(trackId, asNoTracking: false);
            if (track is null)
            {
                return new OperationResultDto { ErrorMessage = "Track not found." };
            }

            if (!isAdmin && track.UploadedByUserId != userId)
            {
                return new OperationResultDto { ErrorMessage = "You do not have permission to delete this upload." };
            }

            var fullPath = Path.Combine(_environment.WebRootPath, track.FilePath.Replace("/", Path.DirectorySeparatorChar.ToString()));
            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
            }

            if (!string.IsNullOrWhiteSpace(track.CoverImagePath))
            {
                var coverPath = Path.Combine(_environment.WebRootPath, track.CoverImagePath.Replace("/", Path.DirectorySeparatorChar.ToString()));
                if (File.Exists(coverPath))
                {
                    File.Delete(coverPath);
                }
            }

            _musicRepository.Remove(track);
            await _musicRepository.SaveChangesAsync();
            return new OperationResultDto { Succeeded = true };
        }
    }
}

