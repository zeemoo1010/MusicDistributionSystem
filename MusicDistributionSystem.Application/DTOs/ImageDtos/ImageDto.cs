using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using MusicDistributionSystem.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace MusicDistributionSystem.Application.DTOs.ImageDtos
{
    public class ImageCardDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public string? CategoryName { get; set; }
        public string FilePath { get; set; } = string.Empty;
        public string? ThumbnailPath { get; set; }
        public int? Width { get; set; }
        public int? Height { get; set; }
        public int DownloadCount { get; set; }
        public ContentAccessLevel AccessLevel { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class ImageDetailsDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public string? CategoryName { get; set; }
        public string? Description { get; set; }
        public string FilePath { get; set; } = string.Empty;
        public string? ThumbnailPath { get; set; }
        public long FileSizeBytes { get; set; }
        public int? Width { get; set; }
        public int? Height { get; set; }
        public int DownloadCount { get; set; }
        public ContentAccessLevel AccessLevel { get; set; }
        public string UploadedByName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }

    public class ImageIndexDto
    {
        public IReadOnlyCollection<ImageCardDto> Images { get; set; } = Array.Empty<ImageCardDto>();
        public IReadOnlyCollection<MusicDistributionSystem.Application.DTOs.Common.CategoryOptionDto> Categories { get; set; } = Array.Empty<MusicDistributionSystem.Application.DTOs.Common.CategoryOptionDto>();
        public string? SearchTerm { get; set; }
        public Guid? CategoryId { get; set; }
        public int TotalCount { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 16;
    }

    public class ImageUploadRequestDto
    {
        [Required]
        [StringLength(150)]
        public string Title { get; set; } = string.Empty;

        public Guid? CategoryId { get; set; }

        [StringLength(1000)]
        public string? Description { get; set; }

        public ContentAccessLevel AccessLevel { get; set; } = ContentAccessLevel.Free;

        [Required]
        public IFormFile? ImageFile { get; set; }

        public List<SelectListItem> Categories { get; set; } = new();
    }
}
