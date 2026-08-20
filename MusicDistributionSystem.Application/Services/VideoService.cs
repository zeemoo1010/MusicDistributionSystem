using Microsoft.AspNetCore.Mvc.Rendering;
using MusicDistributionSystem.Application.Contracts.Infrastructure;
using MusicDistributionSystem.Application.Contracts.Security;
using MusicDistributionSystem.Application.Contracts.Services;
using MusicDistributionSystem.Application.DTOs.Account;
using MusicDistributionSystem.Application.DTOs.Common;
using MusicDistributionSystem.Application.DTOs.Music;
using MusicDistributionSystem.Application.DTOs.VideoDtos;
using MusicDistributionSystem.Domain.Common;
using MusicDistributionSystem.Domain.Contracts.Interface;
using MusicDistributionSystem.Domain.Entities;
using MusicDistributionSystem.Domain.Enums;

namespace MusicDistributionSystem.Application.Services
{
    public class VideoService : IVideoService
    {
        private readonly IVideoRepository _videoRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly IArtistRepository _artistRepository;
        private readonly IFileStorageService _fileStorageService;
        private readonly IUploadedFileSecurityService _fileSecurityService;
        private readonly IUserRepository _userRepository;

        public VideoService(
            IVideoRepository videoRepository,
            ICategoryRepository categoryRepository,
            IArtistRepository artistRepository,
            IFileStorageService fileStorageService,
            IUploadedFileSecurityService fileSecurityService,
            IUserRepository userRepository)
        {
            _videoRepository = videoRepository;
            _categoryRepository = categoryRepository;
            _artistRepository = artistRepository;
            _fileStorageService = fileStorageService;
            _fileSecurityService = fileSecurityService;
            _userRepository = userRepository;
        }

        public async Task<VideoIndexDto> GetVideoIndexAsync(string? searchTerm, Guid? categoryId, int page = 1, int pageSize = 12)
        {
            var paged = await _videoRepository.GetApprovedVideosPagedAsync(searchTerm, categoryId, page, pageSize);
            var categories = await _categoryRepository.GetAllAsync();

            return new VideoIndexDto
            {
                Videos = paged.Items.Select(MapVideoCard).ToList(),
                Categories = categories.Select(c => new CategoryOptionDto { Id = c.Id, Name = c.Name }).ToList(),
                SearchTerm = searchTerm,
                CategoryId = categoryId,
                TotalCount = paged.TotalCount,
                Page = paged.Page,
                PageSize = paged.PageSize
            };
        }

        public async Task<IReadOnlyCollection<VideoCardDto>> GetFeaturedVideosAsync(int take = 6)
        {
            var videos = await _videoRepository.GetFeaturedVideosAsync(take);
            return videos.Select(MapVideoCard).ToList();
        }

        public async Task<VideoDetailsDto?> GetVideoDetailsAsync(Guid id)
        {
            var video = await _videoRepository.GetApprovedVideoByIdAsync(id);
            if (video is null) return null;
            return MapVideoDetails(video);
        }

        public async Task<VideoDetailsDto?> GetVideoDetailsBySlugAsync(string slug)
        {
            var video = await _videoRepository.GetApprovedVideoBySlugAsync(slug);
            if (video is null) return null;
            return MapVideoDetails(video);
        }

        public async Task<VideoUploadRequestDto> GetUploadFormAsync()
        {
            var categories = await _categoryRepository.GetAllAsync();
            return new VideoUploadRequestDto
            {
                Categories = categories.Select(c => new SelectListItem(c.Name, c.Id.ToString())).ToList()
            };
        }

        public async Task<OperationResultDto> UploadAsync(
            VideoUploadRequestDto request,
            Guid uploaderUserId,
            string uploaderName,
            string uploaderEmail)
        {
            if (request.VideoFile is null || request.VideoFile.Length == 0)
            {
                return new OperationResultDto { ErrorMessage = "Video file is required." };
            }

            if (!request.CategoryId.HasValue)
            {
                return new OperationResultDto { ErrorMessage = "Please select a category." };
            }

            var baseSlug = SlugHelper.GenerateSlug($"{request.Title}-{request.ArtistName}");
            var uniqueSlug = $"{baseSlug}-{Guid.NewGuid().ToString("N")[..6]}";

            // Save video file
            var videoFileName = $"vid-{uniqueSlug}.mp4";
            var videoPath = await _fileStorageService.SaveFileAsync(request.VideoFile, "videos", videoFileName);

            // Save thumbnail if provided
            string? thumbPath = null;
            if (request.ThumbnailImage is not null)
            {
                thumbPath = await _fileStorageService.SaveFileAsync(request.ThumbnailImage, "thumbnails", $"thumb-{uniqueSlug}.jpg");
            }

            var video = new Video
            {
                Title = request.Title.Trim(),
                ArtistName = request.ArtistName.Trim(),
                ArtistId = request.ArtistId,
                CategoryId = request.CategoryId.Value,
                Description = request.Description?.Trim(),
                Slug = uniqueSlug,
                FilePath = videoPath,
                OriginalFileName = request.VideoFile.FileName,
                ThumbnailPath = thumbPath,
                FileSizeBytes = request.VideoFile.Length,
                MimeType = request.VideoFile.ContentType,
                AccessLevel = request.AccessLevel,
                ApprovalStatus = ApprovalStatus.Approved, // Default auto-approve for seamless testing, can be toggled
                UploadedByUserId = uploaderUserId,
                UploadedByName = uploaderName,
                UploadedByEmail = uploaderEmail,
                IsFeatured = true
            };

            await _videoRepository.AddAsync(video);
            await _videoRepository.SaveChangesAsync();

            return new OperationResultDto { Succeeded = true };
        }

        public async Task<MusicDownloadResultDto> PrepareDownloadAsync(Guid id, Guid? userId, string? downloaderIpAddress)
        {
            var video = await _videoRepository.GetApprovedVideoByIdAsync(id, asNoTracking: false);
            if (video is null)
            {
                return new MusicDownloadResultDto { Found = false, Allowed = false, ErrorMessage = "Video not found or pending review." };
            }

            // Enforce access level
            if (video.AccessLevel != ContentAccessLevel.Free)
            {
                if (!userId.HasValue)
                {
                    return new MusicDownloadResultDto { Found = true, Allowed = false, ErrorMessage = "Please sign in to access premium videos." };
                }

                var user = await _userRepository.GetByIdAsync(userId.Value);
                if (user is null || user.MembershipTier < (MembershipTier)video.AccessLevel)
                {
                    return new MusicDownloadResultDto { Found = true, Allowed = false, ErrorMessage = "This video requires a higher membership tier." };
                }
            }

            var physicalPath = _fileStorageService.GetPhysicalPath(video.FilePath);
            if (!File.Exists(physicalPath))
            {
                return new MusicDownloadResultDto { Found = true, Allowed = true, FileExists = false, ErrorMessage = "Video file is missing on server." };
            }

            video.DownloadCount++;
            await _videoRepository.SaveChangesAsync();

            return new MusicDownloadResultDto
            {
                Found = true,
                Allowed = true,
                FileExists = true,
                FilePath = physicalPath,
                OriginalFileName = $"{SlugHelper.GenerateSlug(video.Title)}.mp4",
                ContentType = video.MimeType ?? "video/mp4"
            };
        }


        public async Task<OperationResultDto> AddCommentAsync(Guid videoId, Guid userId, string content)
        {
            if (string.IsNullOrWhiteSpace(content))
            {
                return new OperationResultDto { ErrorMessage = "Comment cannot be empty." };
            }

            var video = await _videoRepository.GetApprovedVideoByIdAsync(videoId, asNoTracking: false);
            if (video is null) return new OperationResultDto { ErrorMessage = "Video not found." };

            video.Comments.Add(new Comment
            {
                VideoId = videoId,
                UserId = userId,
                Content = content.Trim(),
                IsApproved = true
            });

            await _videoRepository.SaveChangesAsync();
            return new OperationResultDto { Succeeded = true };
        }

        public async Task<OperationResultDto> ToggleLikeAsync(Guid videoId, Guid userId)
        {
            var video = await _videoRepository.GetApprovedVideoByIdAsync(videoId, asNoTracking: false);
            if (video is null) return new OperationResultDto { ErrorMessage = "Video not found." };

            var existing = video.Likes.FirstOrDefault(l => l.UserId == userId);
            if (existing is not null)
            {
                video.Likes.Remove(existing);
            }
            else
            {
                video.Likes.Add(new Like { VideoId = videoId, UserId = userId });
            }

            await _videoRepository.SaveChangesAsync();
            return new OperationResultDto { Succeeded = true };
        }

        private static VideoCardDto MapVideoCard(Video v) => new()
        {
            Id = v.Id,
            Title = v.Title,
            Slug = v.Slug,
            ArtistName = v.Artist?.Name ?? v.ArtistName,
            CategoryName = v.Category?.Name,
            ThumbnailPath = v.ThumbnailPath,
            Duration = v.Duration,
            ViewCount = v.ViewCount,
            DownloadCount = v.DownloadCount,
            AccessLevel = v.AccessLevel,
            IsFeatured = v.IsFeatured,
            CreatedAt = v.CreatedAt
        };

        private static VideoDetailsDto MapVideoDetails(Video v) => new()
        {
            Id = v.Id,
            Title = v.Title,
            Slug = v.Slug,
            ArtistId = v.ArtistId,
            ArtistName = v.Artist?.Name ?? v.ArtistName,
            CategoryName = v.Category?.Name,
            Description = v.Description,
            FilePath = v.FilePath,
            ThumbnailPath = v.ThumbnailPath,
            Duration = v.Duration,
            FileSizeBytes = v.FileSizeBytes,
            ViewCount = v.ViewCount,
            DownloadCount = v.DownloadCount,
            AccessLevel = v.AccessLevel,
            UploadedByName = v.UploadedByName,
            CreatedAt = v.CreatedAt,
            LikeCount = v.Likes.Count,
            Comments = v.Comments.Select(c => new VideoCommentDto
            {
                Id = c.Id,
                Username = c.User?.Username ?? "User",
                Content = c.Content,
                CreatedAt = c.CreatedAt
            }).ToList()
        };
    }
}
