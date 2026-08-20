using Microsoft.AspNetCore.Mvc.Rendering;
using MusicDistributionSystem.Application.Contracts.Infrastructure;
using MusicDistributionSystem.Application.Contracts.Security;
using MusicDistributionSystem.Application.Contracts.Services;
using MusicDistributionSystem.Application.DTOs.Account;
using MusicDistributionSystem.Application.DTOs.Common;
using MusicDistributionSystem.Application.DTOs.Music;
using MusicDistributionSystem.Domain.Common;
using MusicDistributionSystem.Domain.Contracts.Interface;
using MusicDistributionSystem.Domain.Contracts.Logging;
using MusicDistributionSystem.Domain.Entities;
using MusicDistributionSystem.Domain.Enums;


namespace MusicDistributionSystem.Application.Services
{
    public class MusicService : IMusicService
    {
        private readonly IMusicRepository _musicRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly IUserRepository _userRepository;
        private readonly IUploadedFileSecurityService _uploadedFileSecurityService;
        private readonly IFileStorageService _fileStorageService;
        private readonly IAppLogger _appLogger;

        public MusicService(
            IMusicRepository musicRepository,
            ICategoryRepository categoryRepository,
            IUserRepository userRepository,
            IUploadedFileSecurityService uploadedFileSecurityService,
            IFileStorageService fileStorageService,
            IAppLogger appLogger)
        {
            _musicRepository = musicRepository;
            _categoryRepository = categoryRepository;
            _userRepository = userRepository;
            _uploadedFileSecurityService = uploadedFileSecurityService;
            _fileStorageService = fileStorageService;
            _appLogger = appLogger;
        }

        public async Task<MusicIndexDto> GetMusicIndexAsync(string? searchTerm, Guid? categoryId, int page = 1, int pageSize = 12)
        {
            var paged = await _musicRepository.GetApprovedTracksPagedAsync(searchTerm, categoryId, page, pageSize);
            var categories = await _categoryRepository.GetAllAsync();

            return new MusicIndexDto
            {
                Tracks = paged.Items.Select(MapMusicCard).ToList(),
                Categories = categories.Select(category => new CategoryOptionDto
                {
                    Id = category.Id,
                    Name = category.Name
                }).ToList(),
                SearchTerm = searchTerm,
                CategoryId = categoryId,
                TotalCount = paged.TotalCount,
                Page = paged.Page,
                PageSize = paged.PageSize
            };
        }

        public async Task<IReadOnlyCollection<MusicCardDto>> GetTrendingTracksAsync(int take = 10)
        {
            var tracks = await _musicRepository.GetTrendingTracksAsync(take);
            return tracks.Select(MapMusicCard).ToList();
        }

        public async Task<IReadOnlyCollection<MusicCardDto>> GetTopChartTracksAsync(int take = 10)
        {
            var tracks = await _musicRepository.GetTopChartTracksAsync(take);
            return tracks.Select(MapMusicCard).ToList();
        }

        public async Task<MusicDetailsDto?> GetMusicDetailsAsync(Guid id)
        {
            var track = await _musicRepository.GetApprovedTrackByIdAsync(id);
            if (track is null) return null;
            return MapMusicDetails(track);
        }

        public async Task<MusicDetailsDto?> GetMusicDetailsBySlugAsync(string slug)
        {
            var track = await _musicRepository.GetApprovedTrackBySlugAsync(slug);
            if (track is null) return null;
            return MapMusicDetails(track);
        }

        public async Task<OperationResultDto> AddCommentAsync(Guid trackId, Guid userId, string content)
        {
            if (string.IsNullOrWhiteSpace(content))
            {
                return new OperationResultDto { ErrorMessage = "Comment content cannot be empty." };
            }

            var track = await _musicRepository.GetApprovedTrackByIdAsync(trackId, asNoTracking: false);
            if (track is null) return new OperationResultDto { ErrorMessage = "Track not found." };

            track.Comments.Add(new Comment
            {
                MusicTrackId = trackId,
                UserId = userId,
                Content = content.Trim(),
                IsApproved = true
            });

            await _musicRepository.SaveChangesAsync();
            return new OperationResultDto { Succeeded = true };
        }

        public async Task<OperationResultDto> ToggleLikeAsync(Guid trackId, Guid userId)
        {
            var track = await _musicRepository.GetApprovedTrackByIdAsync(trackId, asNoTracking: false);
            if (track is null) return new OperationResultDto { ErrorMessage = "Track not found." };

            var existing = track.Likes.FirstOrDefault(l => l.UserId == userId);
            if (existing is not null)
            {
                track.Likes.Remove(existing);
            }
            else
            {
                track.Likes.Add(new Like { MusicTrackId = trackId, UserId = userId });
            }

            await _musicRepository.SaveChangesAsync();
            return new OperationResultDto { Succeeded = true };
        }

        public async Task<MusicUploadRequestDto> GetUploadFormAsync()
        {
            var request = new MusicUploadRequestDto();
            await PopulateCategoriesAsync(request);
            return request;
        }

        public async Task<MusicUploadResultDto> UploadAsync(MusicUploadRequestDto request, Guid uploaderUserId, string uploaderName, string uploaderEmail)
        {
            if (request.MusicFile is null)
            {
                return new MusicUploadResultDto
                {
                    ErrorMessage = "Please select an MP3 file to upload."
                };
            }

            var validation = await _uploadedFileSecurityService.ValidateMusicFileAsync(request.MusicFile);
            if (!validation.IsValid)
            {
                return new MusicUploadResultDto
                {
                    ErrorMessage = validation.ErrorMessage ?? "The uploaded file did not pass validation."
                };
            }

            var coverImageValidation = await _uploadedFileSecurityService.ValidateCoverImageAsync(request.CoverImage);
            if (!coverImageValidation.IsValid)
            {
                return new MusicUploadResultDto
                {
                    ErrorMessage = coverImageValidation.ErrorMessage ?? "The uploaded cover image did not pass validation."
                };
            }

            var sanitizedAudioName = _uploadedFileSecurityService.SanitizeFileName(request.MusicFile.FileName);
            var audioRelativePath = await _fileStorageService.SaveFileAsync(request.MusicFile, "music", sanitizedAudioName);

            string? coverImageRelativePath = null;
            if (request.CoverImage is not null)
            {
                var sanitizedCoverName = _uploadedFileSecurityService.SanitizeFileName(request.CoverImage.FileName);
                coverImageRelativePath = await _fileStorageService.SaveFileAsync(request.CoverImage, "covers", sanitizedCoverName);
            }

            var generatedSlug = SlugHelper.GenerateSlug($"{request.Artist}-{request.Title}");

            var track = new MusicTrack
            {
                Title = request.Title.Trim(),
                Artist = request.Artist.Trim(),
                Slug = generatedSlug,
                Description = request.Description?.Trim(),
                CategoryId = request.CategoryId!.Value,
                AccessLevel = request.AccessLevel,
                UploadedByUserId = uploaderUserId,
                UploadedByName = uploaderName.Trim(),
                UploadedByEmail = uploaderEmail.Trim(),
                OriginalFileName = sanitizedAudioName,
                CoverImagePath = coverImageRelativePath,
                FilePath = audioRelativePath,
                FileSizeBytes = request.MusicFile.Length,
                ApprovalStatus = ApprovalStatus.Approved, // Auto approve for instant testing and discovery
                IsFeatured = true
            };

            await _musicRepository.AddAsync(track);
            await _musicRepository.SaveChangesAsync();
            await _appLogger.LogInformationAsync("Music", $"Track '{track.Title}' uploaded by '{track.UploadedByEmail}'.");

            return new MusicUploadResultDto
            {
                Succeeded = true
            };
        }

        public async Task<MusicDownloadResultDto> PrepareDownloadAsync(Guid id, Guid? userId, string? downloaderIpAddress)
        {
            var track = await _musicRepository.GetApprovedTrackByIdAsync(id, asNoTracking: false);
            if (track is null)
            {
                return new MusicDownloadResultDto
                {
                    Found = false
                };
            }

            if (track.AccessLevel != ContentAccessLevel.Free)
            {
                if (userId is null)
                {
                    await _appLogger.LogWarningAsync("Music", $"Blocked anonymous premium download attempt for track '{track.Id}'.");
                    return new MusicDownloadResultDto
                    {
                        Found = true,
                        Allowed = false,
                        ErrorMessage = "Please log in with a matching membership tier to download this release."
                    };
                }

                var user = await _userRepository.GetByIdAsync(userId.Value);
                if (user is null || user.MembershipTier < (MembershipTier)track.AccessLevel)
                {
                    await _appLogger.LogWarningAsync("Music", $"Blocked premium download attempt for user '{userId}' on track '{track.Id}'.");
                    return new MusicDownloadResultDto
                    {
                        Found = true,
                        Allowed = false,
                        ErrorMessage = $"This release requires the {(MembershipTier)track.AccessLevel} membership tier."
                    };
                }
            }

            if (!_fileStorageService.FileExists(track.FilePath))
            {
                return new MusicDownloadResultDto
                {
                    Found = true,
                    Allowed = true,
                    FileExists = false,
                    ErrorMessage = "This file is currently unavailable on the server."
                };
            }

            var fullPath = _fileStorageService.GetPhysicalPath(track.FilePath);

            track.DownloadCount += 1;
            await _musicRepository.AddDownloadRecordAsync(new DownloadRecord
            {
                MusicTrackId = track.Id,
                DownloaderIpAddress = downloaderIpAddress
            });
            await _musicRepository.SaveChangesAsync();
            await _appLogger.LogInformationAsync("Music", $"Download served for track '{track.Id}' to '{downloaderIpAddress ?? "unknown-ip"}'.");

            return new MusicDownloadResultDto
            {
                Found = true,
                Allowed = true,
                FileExists = true,
                FilePath = fullPath,
                OriginalFileName = track.OriginalFileName
            };
        }

        private async Task PopulateCategoriesAsync(MusicUploadRequestDto request)
        {
            var categories = await _categoryRepository.GetAllAsync();
            request.Categories = categories
                .Select(category => new SelectListItem
                {
                    Value = category.Id.ToString(),
                    Text = category.Name
                })
                .ToList();
        }

        private static MusicCardDto MapMusicCard(MusicTrack track)
        {
            return new MusicCardDto
            {
                Id = track.Id,
                Title = track.Title,
                Artist = track.ArtistEntity?.Name ?? track.Artist,
                Slug = track.Slug,
                ArtistId = track.ArtistId,
                AlbumId = track.AlbumId,
                AlbumTitle = track.Album?.Title,
                Description = track.Description,
                CategoryName = track.Category?.Name,
                CoverImagePath = track.CoverImagePath,
                Duration = track.Duration,
                AccessLevel = track.AccessLevel,
                DownloadCount = track.DownloadCount,
                PlayCount = track.PlayCount,
                LikeCount = track.Likes.Count,
                CreatedAt = track.CreatedAt,
                IsFeatured = track.IsFeatured
            };
        }

        private static MusicDetailsDto MapMusicDetails(MusicTrack track)
        {
            return new MusicDetailsDto
            {
                Id = track.Id,
                Title = track.Title,
                Artist = track.ArtistEntity?.Name ?? track.Artist,
                Slug = track.Slug,
                ArtistId = track.ArtistId,
                AlbumId = track.AlbumId,
                AlbumTitle = track.Album?.Title,
                Description = track.Description,
                CategoryName = track.Category?.Name,
                CategoryId = track.CategoryId,
                CoverImagePath = track.CoverImagePath,
                Duration = track.Duration,
                UploadedByName = track.UploadedByName,
                AccessLevel = track.AccessLevel,
                DownloadCount = track.DownloadCount,
                PlayCount = track.PlayCount,
                LikeCount = track.Likes.Count,
                FileSizeBytes = track.FileSizeBytes,
                CreatedAt = track.CreatedAt,
                Comments = track.Comments.Select(c => new MusicCommentDto
                {
                    Id = c.Id,
                    Username = c.User?.Username ?? "User",
                    Content = c.Content,
                    CreatedAt = c.CreatedAt
                }).ToList()
            };
        }
    }
}


