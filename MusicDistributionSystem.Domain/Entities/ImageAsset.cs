using MusicDistributionSystem.Domain.Common;
using MusicDistributionSystem.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace MusicDistributionSystem.Domain.Entities
{
    public class ImageAsset : BaseEntity
    {
        [Required]
        [StringLength(150)]
        public string Title { get; set; } = string.Empty;

        [StringLength(180)]
        public string Slug { get; set; } = string.Empty;

        [StringLength(1000)]
        public string? Description { get; set; }

        [Required]
        public string FilePath { get; set; } = string.Empty;

        public string? ThumbnailPath { get; set; }

        public long FileSizeBytes { get; set; }

        public string? MimeType { get; set; }

        public int? Width { get; set; }

        public int? Height { get; set; }

        public Guid? CategoryId { get; set; }
        public Category? Category { get; set; }

        public Guid UploadedByUserId { get; set; }
        public User? UploadedByUser { get; set; }

        [StringLength(100)]
        public string UploadedByName { get; set; } = string.Empty;

        [EmailAddress]
        [StringLength(150)]
        public string UploadedByEmail { get; set; } = string.Empty;

        public ApprovalStatus ApprovalStatus { get; set; } = ApprovalStatus.Approved;

        public ContentAccessLevel AccessLevel { get; set; } = ContentAccessLevel.Free;

        public int DownloadCount { get; set; }

        public int ViewCount { get; set; }

        public bool IsFeatured { get; set; }

        public Guid? ReviewedByUserId { get; set; }
        public User? ReviewedByUser { get; set; }

        public DateTime? ReviewedAtUtc { get; set; }

        [StringLength(500)]
        public string? RejectionReason { get; set; }
    }
}


