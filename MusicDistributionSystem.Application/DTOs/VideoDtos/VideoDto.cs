using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using MusicDistributionSystem.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace MusicDistributionSystem.Application.DTOs.VideoDtos
{
    public class VideoCardDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public string ArtistName { get; set; } = string.Empty;
        public string? CategoryName { get; set; }
        public string? ThumbnailPath { get; set; }
        public string? Duration { get; set; }
        public int ViewCount { get; set; }
        public int DownloadCount { get; set; }
        public ContentAccessLevel AccessLevel { get; set; }
        public bool IsFeatured { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class VideoDetailsDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public string ArtistName { get; set; } = string.Empty;
        public Guid? ArtistId { get; set; }
        public string? CategoryName { get; set; }
        public string? Description { get; set; }
        public string FilePath { get; set; } = string.Empty;
        public string? ThumbnailPath { get; set; }
        public string? Duration { get; set; }
        public long FileSizeBytes { get; set; }
        public int ViewCount { get; set; }
        public int DownloadCount { get; set; }
        public ContentAccessLevel AccessLevel { get; set; }
        public string UploadedByName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public int LikeCount { get; set; }
        public IReadOnlyCollection<VideoCommentDto> Comments { get; set; } = Array.Empty<VideoCommentDto>();
    }

    public class VideoCommentDto
    {
        public Guid Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }

    public class VideoIndexDto
    {
        public IReadOnlyCollection<VideoCardDto> Videos { get; set; } = Array.Empty<VideoCardDto>();
        public IReadOnlyCollection<MusicDistributionSystem.Application.DTOs.Common.CategoryOptionDto> Categories { get; set; } = Array.Empty<MusicDistributionSystem.Application.DTOs.Common.CategoryOptionDto>();
        public string? SearchTerm { get; set; }
        public Guid? CategoryId { get; set; }
        public int TotalCount { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 12;
    }

    public class VideoUploadRequestDto
    {
        [Required]
        [StringLength(180)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [StringLength(120)]
        public string ArtistName { get; set; } = string.Empty;

        public Guid? ArtistId { get; set; }

        [Required]
        public Guid? CategoryId { get; set; }

        [StringLength(2000)]
        public string? Description { get; set; }

        public ContentAccessLevel AccessLevel { get; set; } = ContentAccessLevel.Free;

        [Required]
        public IFormFile? VideoFile { get; set; }

        public IFormFile? ThumbnailImage { get; set; }

        public List<SelectListItem> Categories { get; set; } = new();
    }
}
