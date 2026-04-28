using Microsoft.AspNetCore.Mvc.Rendering;
using MusicDistributionSystem.Application.DTOs.Music;
using MusicDistributionSystem.Domain.Enums;
using MusicDistributionSystem.Infrastructure.Logging;
using MusicDistributionSystem.Domain.Entities;
using MusicDistributionSystem.Application.Contracts.Repositories;
using MusicDistributionSystem.Application.Contracts.Services;
using MusicDistributionSystem.Application.Contracts.Security;

namespace MusicDistributionSystem.Application.Services
{
    public class MusicService : IMusicService
    {
        private readonly IMusicRepository _musicRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly IUploadedFileSecurityService _uploadedFileSecurityService;
        private readonly IWebHostEnvironment _environment;
        private readonly IAppLogger _appLogger;

        public MusicService(
            IMusicRepository musicRepository,
            ICategoryRepository categoryRepository,
            IUploadedFileSecurityService uploadedFileSecurityService,
            IWebHostEnvironment environment,
            IAppLogger appLogger)
        {
            _musicRepository = musicRepository;
            _categoryRepository = categoryRepository;
            _uploadedFileSecurityService = uploadedFileSecurityService;
            _environment = environment;
            _appLogger = appLogger;
        }

        public async Task<MusicIndexDto> GetMusicIndexAsync(string? searchTerm, Guid? categoryId)
        {
            var tracks = await _musicRepository.GetApprovedTracksAsync(searchTerm, categoryId);
            var categories = await _categoryRepository.GetAllAsync();

            return new MusicIndexDto
            {
                Tracks = tracks.Select(MapMusicCard).ToList(),
                Categories = categories.Select(category => new DTOs.Common.CategoryOptionDto
                {
                    Id = category.Id,
                    Name = category.Name
                }).ToList(),
                SearchTerm = searchTerm,
                CategoryId = categoryId
            };
        }

        public async Task<MusicDetailsDto?> GetMusicDetailsAsync(Guid id)
        {
            var track = await _musicRepository.GetApprovedTrackByIdAsync(id);
            if (track is null)
            {
                return null;
            }

            return new MusicDetailsDto
            {
                Id = track.Id,
                Title = track.Title,
                Artist = track.Artist,
                Description = track.Description,
                CategoryName = track.Category?.Name,
                UploadedByName = track.UploadedByName,
                AccessLevel = track.AccessLevel,
                DownloadCount = track.DownloadCount,
                FileSizeBytes = track.FileSizeBytes,
                CreatedAt = track.CreatedAt
            };
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

            var extension = Path.GetExtension(request.MusicFile.FileName).ToLowerInvariant();
            var uploadsRoot = Path.Combine(_environment.WebRootPath, "uploads", "music");
            Directory.CreateDirectory(uploadsRoot);

            var storedFileName = $"{Guid.NewGuid():N}{extension}";
            var fullPath = Path.Combine(uploadsRoot, storedFileName);

            await using (var stream = new FileStream(fullPath, FileMode.CreateNew, FileAccess.Write, FileShare.None))
            {
                await request.MusicFile.CopyToAsync(stream);
            }

            var track = new MusicTrack
            {
                Title = request.Title.Trim(),
                Artist = request.Artist.Trim(),
                Description = request.Description?.Trim(),
                CategoryId = request.CategoryId!.Value,
                AccessLevel = request.AccessLevel,
                UploadedByUserId = uploaderUserId,
                UploadedByName = uploaderName.Trim(),
                UploadedByEmail = uploaderEmail.Trim(),
                OriginalFileName = Path.GetFileName(request.MusicFile.FileName),
                FilePath = Path.Combine("uploads", "music", storedFileName).Replace("\\", "/"),
                FileSizeBytes = request.MusicFile.Length,
                ApprovalStatus = ApprovalStatus.Pending
            };

            await _musicRepository.AddAsync(track);
            await _musicRepository.SaveChangesAsync();
            await _appLogger.LogInformationAsync("Music", $"Track '{track.Title}' uploaded by '{track.UploadedByEmail}' and queued for approval.");

            return new MusicUploadResultDto
            {
                Succeeded = true
            };
        }

        public async Task<MusicDownloadResultDto> PrepareDownloadAsync(Guid id, string? downloaderIpAddress)
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
                await _appLogger.LogWarningAsync("Music", $"Blocked premium download attempt for track '{track.Id}'.");
                return new MusicDownloadResultDto
                {
                    Found = true,
                    Allowed = false,
                    ErrorMessage = "This release is reserved for paid membership tiers. Connect authentication and subscriptions next to unlock gated downloads."
                };
            }

            var fullPath = Path.Combine(_environment.WebRootPath, track.FilePath.Replace("/", Path.DirectorySeparatorChar.ToString()));
            if (!System.IO.File.Exists(fullPath))
            {
                return new MusicDownloadResultDto
                {
                    Found = true,
                    Allowed = true,
                    FileExists = false,
                    ErrorMessage = "This file is currently unavailable on the server."
                };
            }

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
                Artist = track.Artist,
                Description = track.Description,
                CategoryName = track.Category?.Name,
                AccessLevel = track.AccessLevel,
                DownloadCount = track.DownloadCount,
                CreatedAt = track.CreatedAt,
                IsFeatured = track.IsFeatured
            };
        }
    }
}

