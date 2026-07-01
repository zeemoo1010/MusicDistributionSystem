using MusicDistributionSystem.Domain.Common;
using MusicDistributionSystem.Domain.Enums;

namespace MusicDistributionSystem.Domain.Entities
{
    public class MediaAsset : BaseEntity
    {
        public string Title { get; set; } = string.Empty;
        public MediaType Type { get; set; }
        public string FilePath { get; set; } = string.Empty;
        public string OriginalFileName { get; set; } = string.Empty;
        public long FileSizeBytes { get; set; }
        public string? MimeType { get; set; }
        public string? ThumbnailPath { get; set; }
        public string? Duration { get; set; }
        public string? Resolution { get; set; }
        public ApprovalStatus ApprovalStatus { get; set; } = ApprovalStatus.Pending;
        public ContentAccessLevel AccessLevel { get; set; } = ContentAccessLevel.Free;
        public int DownloadCount { get; set; }
        public int ViewCount { get; set; }
        public bool IsFeatured { get; set; }

        public Guid CategoryId { get; set; }
        public Category Category { get; set; } = null!;

        public Guid UploadedByUserId { get; set; }
        public User UploadedByUser { get; set; } = null!;

        public string UploadedByName { get; set; } = string.Empty;
        public string UploadedByEmail { get; set; } = string.Empty;

        public Guid? ReviewedByUserId { get; set; }
        public User? ReviewedByUser { get; set; }

        public DateTime? ReviewedAtUtc { get; set; }
        public string? RejectionReason { get; set; }

        public ICollection<DownloadRecord> Downloads { get; set; } = new List<DownloadRecord>();
        public ICollection<Comment> Comments { get; set; } = new List<Comment>();
        public ICollection<Like> Likes { get; set; } = new List<Like>();
        public ICollection<MediaAssetTag> MediaAssetTags { get; set; } = new List<MediaAssetTag>();
    }
}