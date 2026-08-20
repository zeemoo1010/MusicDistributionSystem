using Microsoft.AspNetCore.Mvc.Rendering;
using MusicDistributionSystem.Application.Contracts.Infrastructure;
using MusicDistributionSystem.Application.Contracts.Services;
using MusicDistributionSystem.Application.DTOs.Account;
using MusicDistributionSystem.Application.DTOs.Common;
using MusicDistributionSystem.Application.DTOs.ImageDtos;
using MusicDistributionSystem.Application.DTOs.Music;
using MusicDistributionSystem.Domain.Common;
using MusicDistributionSystem.Domain.Contracts.Interface;
using MusicDistributionSystem.Domain.Entities;
using MusicDistributionSystem.Domain.Enums;

namespace MusicDistributionSystem.Application.Services
{
    public class ImageService : IImageService
    {
        private readonly IImageAssetRepository _imageRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly IFileStorageService _fileStorageService;
        private readonly IUserRepository _userRepository;

        public ImageService(
            IImageAssetRepository imageRepository,
            ICategoryRepository categoryRepository,
            IFileStorageService fileStorageService,
            IUserRepository userRepository)
        {
            _imageRepository = imageRepository;
            _categoryRepository = categoryRepository;
            _fileStorageService = fileStorageService;
            _userRepository = userRepository;
        }

        public async Task<ImageIndexDto> GetImageIndexAsync(string? searchTerm, Guid? categoryId, int page = 1, int pageSize = 16)
        {
            var paged = await _imageRepository.GetApprovedImagesPagedAsync(searchTerm, categoryId, page, pageSize);
            var categories = await _categoryRepository.GetAllAsync();

            return new ImageIndexDto
            {
                Images = paged.Items.Select(MapImageCard).ToList(),
                Categories = categories.Select(c => new CategoryOptionDto { Id = c.Id, Name = c.Name }).ToList(),
                SearchTerm = searchTerm,
                CategoryId = categoryId,
                TotalCount = paged.TotalCount,
                Page = paged.Page,
                PageSize = paged.PageSize
            };
        }

        public async Task<IReadOnlyCollection<ImageCardDto>> GetFeaturedImagesAsync(int take = 8)
        {
            var images = await _imageRepository.GetFeaturedImagesAsync(take);
            return images.Select(MapImageCard).ToList();
        }

        public async Task<ImageDetailsDto?> GetImageDetailsAsync(Guid id)
        {
            var img = await _imageRepository.GetByIdAsync(id);
            if (img is null) return null;
            return MapImageDetails(img);
        }

        public async Task<ImageDetailsDto?> GetImageDetailsBySlugAsync(string slug)
        {
            var img = await _imageRepository.GetBySlugAsync(slug);
            if (img is null) return null;
            return MapImageDetails(img);
        }

        public async Task<ImageUploadRequestDto> GetUploadFormAsync()
        {
            var categories = await _categoryRepository.GetAllAsync();
            return new ImageUploadRequestDto
            {
                Categories = categories.Select(c => new SelectListItem(c.Name, c.Id.ToString())).ToList()
            };
        }

        public async Task<OperationResultDto> UploadAsync(
            ImageUploadRequestDto request,
            Guid uploaderUserId,
            string uploaderName,
            string uploaderEmail)
        {
            if (request.ImageFile is null || request.ImageFile.Length == 0)
            {
                return new OperationResultDto { ErrorMessage = "Image file is required." };
            }

            var baseSlug = SlugHelper.GenerateSlug(request.Title);
            var uniqueSlug = $"{baseSlug}-{Guid.NewGuid().ToString("N")[..6]}";

            var filePath = await _fileStorageService.SaveFileAsync(request.ImageFile, "gallery", $"img-{uniqueSlug}.jpg");

            var asset = new ImageAsset
            {
                Title = request.Title.Trim(),
                Slug = uniqueSlug,
                CategoryId = request.CategoryId,
                Description = request.Description?.Trim(),
                FilePath = filePath,
                FileSizeBytes = request.ImageFile.Length,
                MimeType = request.ImageFile.ContentType,
                ApprovalStatus = ApprovalStatus.Approved,
                AccessLevel = request.AccessLevel,
                UploadedByUserId = uploaderUserId,
                UploadedByName = uploaderName,
                UploadedByEmail = uploaderEmail,
                IsFeatured = true
            };

            await _imageRepository.AddAsync(asset);
            await _imageRepository.SaveChangesAsync();

            return new OperationResultDto { Succeeded = true };
        }

        public async Task<MusicDownloadResultDto> PrepareDownloadAsync(Guid id, Guid? userId, string? downloaderIpAddress)
        {
            var img = await _imageRepository.GetByIdAsync(id, asNoTracking: false);
            if (img is null)
            {
                return new MusicDownloadResultDto { Found = false, Allowed = false, ErrorMessage = "Image not found." };
            }

            if (img.AccessLevel != ContentAccessLevel.Free)
            {
                if (!userId.HasValue)
                {
                    return new MusicDownloadResultDto { Found = true, Allowed = false, ErrorMessage = "Please sign in to download premium images." };
                }

                var user = await _userRepository.GetByIdAsync(userId.Value);
                if (user is null || user.MembershipTier < (MembershipTier)img.AccessLevel)
                {
                    return new MusicDownloadResultDto { Found = true, Allowed = false, ErrorMessage = "This image requires a higher membership tier." };
                }
            }

            var physicalPath = _fileStorageService.GetPhysicalPath(img.FilePath);
            if (!File.Exists(physicalPath))
            {
                return new MusicDownloadResultDto { Found = true, Allowed = true, FileExists = false, ErrorMessage = "Image file is missing on server." };
            }

            img.DownloadCount++;
            await _imageRepository.SaveChangesAsync();

            return new MusicDownloadResultDto
            {
                Found = true,
                Allowed = true,
                FileExists = true,
                FilePath = physicalPath,
                OriginalFileName = $"{SlugHelper.GenerateSlug(img.Title)}.jpg",
                ContentType = img.MimeType ?? "image/jpeg"
            };
        }


        private static ImageCardDto MapImageCard(ImageAsset i) => new()
        {
            Id = i.Id,
            Title = i.Title,
            Slug = i.Slug,
            CategoryName = i.Category?.Name,
            FilePath = i.FilePath,
            ThumbnailPath = i.ThumbnailPath ?? i.FilePath,
            Width = i.Width,
            Height = i.Height,
            DownloadCount = i.DownloadCount,
            AccessLevel = i.AccessLevel,
            CreatedAt = i.CreatedAt
        };

        private static ImageDetailsDto MapImageDetails(ImageAsset i) => new()
        {
            Id = i.Id,
            Title = i.Title,
            Slug = i.Slug,
            CategoryName = i.Category?.Name,
            Description = i.Description,
            FilePath = i.FilePath,
            ThumbnailPath = i.ThumbnailPath,
            FileSizeBytes = i.FileSizeBytes,
            Width = i.Width,
            Height = i.Height,
            DownloadCount = i.DownloadCount,
            AccessLevel = i.AccessLevel,
            UploadedByName = i.UploadedByName,
            CreatedAt = i.CreatedAt
        };
    }
}
