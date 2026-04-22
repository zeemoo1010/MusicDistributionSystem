using System.ComponentModel.DataAnnotations;
using MusicDistributionSystem.Enums;

namespace MusicDistributionSystem.Models
{
    public class MusicTrack
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [StringLength(150)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [StringLength(120)]
        public string Artist { get; set; } = string.Empty;

        [StringLength(1000)]
        public string? Description { get; set; }

        [Required]
        public string FilePath { get; set; } = string.Empty;

        [Required]
        public string OriginalFileName { get; set; } = string.Empty;

        public long FileSizeBytes { get; set; }

        public ApprovalStatus ApprovalStatus { get; set; } = ApprovalStatus.Pending;

        public ContentAccessLevel AccessLevel { get; set; } = ContentAccessLevel.Free;

        public int DownloadCount { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public bool IsFeatured { get; set; }

        public Guid CategoryId { get; set; }
        public Category? Category { get; set; }

        [StringLength(100)]
        public string UploadedByName { get; set; } = string.Empty;

        [EmailAddress]
        [StringLength(150)]
        public string UploadedByEmail { get; set; } = string.Empty;

        public ICollection<DownloadRecord> Downloads { get; set; } = new List<DownloadRecord>();
    }
}
