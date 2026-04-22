using System.ComponentModel.DataAnnotations;

namespace MusicDistributionSystem.Models
{
    public class ImageAsset
    {
        public int Id { get; set; }

        [Required]
        [StringLength(150)]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string FilePath { get; set; } = string.Empty;

        public ApprovalStatus ApprovalStatus { get; set; } = ApprovalStatus.Pending;

        [StringLength(100)]
        public string UploadedByName { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
