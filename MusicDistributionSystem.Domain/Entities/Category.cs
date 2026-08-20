using MusicDistributionSystem.Domain.Common;
using System.ComponentModel.DataAnnotations;

namespace MusicDistributionSystem.Domain.Entities
{
    public class Category : BaseEntity
    {
        [Required]
        [StringLength(80)]
        public string Name { get; set; } = string.Empty;

        [StringLength(100)]
        public string Slug { get; set; } = string.Empty;

        [StringLength(250)]
        public string? Description { get; set; }

        public ICollection<MusicTrack> MusicTracks { get; set; } = new List<MusicTrack>();
        public ICollection<Album> Albums { get; set; } = new List<Album>();
        public ICollection<Video> Videos { get; set; } = new List<Video>();
        public ICollection<ImageAsset> ImageAssets { get; set; } = new List<ImageAsset>();
    }
}


