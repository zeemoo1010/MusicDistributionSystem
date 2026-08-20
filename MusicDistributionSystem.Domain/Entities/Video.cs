using MusicDistributionSystem.Domain.Common;
using MusicDistributionSystem.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace MusicDistributionSystem.Domain.Entities
{
    public class Video : BaseEntity
    {
        [Required]
        [StringLength(180)]
        public string Title { get; set; } = string.Empty;

        [StringLength(220)]
        public string Slug { get; set; } = string.Empty;

        [Required]
        [StringLength(120)]
        public string ArtistName { get; set; } = string.Empty;

        public Guid? ArtistId { get; set; }
        public Artist? Artist { get; set; }

        public Guid CategoryId { get; set; }
        public Category? Category { get; set; }

        [StringLength(2000)]
        public string? Description { get; set; }

        [Required]
        public string FilePath { get; set; } = string.Empty;

        [Required]
        public string OriginalFileName { get; set; } = string.Empty;

        public string? ThumbnailPath { get; set; }

        public long FileSizeBytes { get; set; }

        public string? MimeType { get; set; }

        public string? Duration { get; set; }

        public ApprovalStatus ApprovalStatus { get; set; } = ApprovalStatus.Pending;

        public ContentAccessLevel AccessLevel { get; set; } = ContentAccessLevel.Free;

        public int ViewCount { get; set; }

        public int DownloadCount { get; set; }

        public bool IsFeatured { get; set; }

        public Guid UploadedByUserId { get; set; }
        public User? UploadedByUser { get; set; }

        [StringLength(100)]
        public string UploadedByName { get; set; } = string.Empty;

        [EmailAddress]
        [StringLength(150)]
        public string UploadedByEmail { get; set; } = string.Empty;

        public Guid? ReviewedByUserId { get; set; }
        public User? ReviewedByUser { get; set; }

        public DateTime? ReviewedAtUtc { get; set; }

        [StringLength(500)]
        public string? RejectionReason { get; set; }

        public ICollection<Comment> Comments { get; set; } = new List<Comment>();
        public ICollection<Like> Likes { get; set; } = new List<Like>();
    }
}
