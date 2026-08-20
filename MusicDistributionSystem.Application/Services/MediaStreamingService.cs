using MusicDistributionSystem.Application.Contracts.Infrastructure;
using MusicDistributionSystem.Application.Contracts.Services;
using MusicDistributionSystem.Domain.Contracts.Interface;
using MusicDistributionSystem.Domain.Entities;
using MusicDistributionSystem.Domain.Enums;

namespace MusicDistributionSystem.Application.Services
{
    public class MediaStreamingService : IMediaStreamingService
    {
        private readonly IMusicRepository _musicRepository;
        private readonly IVideoRepository _videoRepository;
        private readonly IUserRepository _userRepository;
        private readonly IFileStorageService _fileStorageService;

        public MediaStreamingService(
            IMusicRepository musicRepository,
            IVideoRepository videoRepository,
            IUserRepository userRepository,
            IFileStorageService fileStorageService)
        {
            _musicRepository = musicRepository;
            _videoRepository = videoRepository;
            _userRepository = userRepository;
            _fileStorageService = fileStorageService;
        }

        public async Task<(bool Found, bool Allowed, string? FilePath, string? ContentType, long FileLength, string? ErrorMessage)> GetMediaStreamAsync(
            Guid id,
            string mediaType,
            Guid? userId)
        {
            if (mediaType.Equals("video", StringComparison.OrdinalIgnoreCase))
            {
                var video = await _videoRepository.GetApprovedVideoByIdAsync(id);
                if (video is null)
                {
                    return (false, false, null, null, 0, "Video not found or pending review.");
                }

                if (video.AccessLevel != ContentAccessLevel.Free)
                {
                    if (!userId.HasValue)
                    {
                        return (true, false, null, null, 0, "Sign in required to stream premium video.");
                    }

                    var user = await _userRepository.GetByIdAsync(userId.Value);
                    if (user is null || user.MembershipTier < (MembershipTier)video.AccessLevel)
                    {
                        return (true, false, null, null, 0, "Upgrade your plan to stream this video.");
                    }
                }

                var physicalPath = _fileStorageService.GetPhysicalPath(video.FilePath);
                if (!File.Exists(physicalPath))
                {
                    return (false, false, null, null, 0, "Media file is missing on server.");
                }

                var fileInfo = new FileInfo(physicalPath);
                return (true, true, physicalPath, video.MimeType ?? "video/mp4", fileInfo.Length, null);
            }
            else
            {
                var track = await _musicRepository.GetApprovedTrackByIdAsync(id);
                if (track is null)
                {
                    return (false, false, null, null, 0, "Track not found or pending review.");
                }

                if (track.AccessLevel != ContentAccessLevel.Free)
                {
                    if (!userId.HasValue)
                    {
                        return (true, false, null, null, 0, "Sign in required to stream premium music.");
                    }

                    var user = await _userRepository.GetByIdAsync(userId.Value);
                    if (user is null || user.MembershipTier < (MembershipTier)track.AccessLevel)
                    {
                        return (true, false, null, null, 0, "Upgrade your plan to stream this track.");
                    }
                }

                var physicalPath = _fileStorageService.GetPhysicalPath(track.FilePath);
                if (!File.Exists(physicalPath))
                {
                    return (false, false, null, null, 0, "Audio file is missing on server.");
                }

                var fileInfo = new FileInfo(physicalPath);
                return (true, true, physicalPath, track.MimeType ?? "audio/mpeg", fileInfo.Length, null);
            }
        }

        public async Task RecordPlayAsync(Guid trackId, Guid? userId, string? ipAddress)
        {
            var track = await _musicRepository.GetApprovedTrackByIdAsync(trackId, asNoTracking: false);
            if (track is not null)
            {
                track.PlayCount++;
                await _musicRepository.SaveChangesAsync();
            }
        }

        public async Task RecordVideoViewAsync(Guid videoId, Guid? userId, string? ipAddress)
        {
            var video = await _videoRepository.GetApprovedVideoByIdAsync(videoId, asNoTracking: false);
            if (video is not null)
            {
                video.ViewCount++;
                await _videoRepository.SaveChangesAsync();
            }
        }
    }
}
