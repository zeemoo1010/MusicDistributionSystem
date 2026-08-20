using MusicDistributionSystem.Domain.Common;
using MusicDistributionSystem.Domain.Enums;
using System.ComponentModel.DataAnnotations;

namespace MusicDistributionSystem.Domain.Entities
{
    public class Album : BaseEntity
    {
        [Required]
        [StringLength(150)]
        public string Title { get; set; } = string.Empty;

        [StringLength(180)]
        public string Slug { get; set; } = string.Empty;

        public Guid ArtistId { get; set; }
        public Artist? Artist { get; set; }

        public Guid CategoryId { get; set; }
        public Category? Category { get; set; }

        public string? CoverImagePath { get; set; }

        [StringLength(1500)]
        public string? Description { get; set; }

        public DateTime? ReleaseDate { get; set; }

        public ContentAccessLevel AccessLevel { get; set; } = ContentAccessLevel.Free;

        public bool IsFeatured { get; set; }

        public ApprovalStatus ApprovalStatus { get; set; } = ApprovalStatus.Approved;

        public Guid UploadedByUserId { get; set; }
        public User? UploadedByUser { get; set; }

        public ICollection<MusicTrack> Tracks { get; set; } = new List<MusicTrack>();
    }
}
