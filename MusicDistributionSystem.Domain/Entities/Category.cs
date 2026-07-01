using MusicDistributionSystem.Domain.Common;
using System.ComponentModel.DataAnnotations;

namespace MusicDistributionSystem.Domain.Entities
{
    public class Category : BaseEntity
    {
        [Required]
        [StringLength(80)]
        public string Name { get; set; } = string.Empty;

        [StringLength(250)]
        public string? Description { get; set; }

        public ICollection<MusicTrack> MusicTracks { get; set; } = new List<MusicTrack>();
        public ICollection<MediaAsset> MediaAssets { get; set; } = new List<MediaAsset>();
    }
}

