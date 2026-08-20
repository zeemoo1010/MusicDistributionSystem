using MusicDistributionSystem.Domain.Common;
using System.ComponentModel.DataAnnotations;

namespace MusicDistributionSystem.Domain.Entities
{
    public class Artist : BaseEntity
    {
        [Required]
        [StringLength(120)]
        public string Name { get; set; } = string.Empty;

        [StringLength(150)]
        public string Slug { get; set; } = string.Empty;

        [StringLength(2000)]
        public string? Bio { get; set; }

        public string? ProfilePicturePath { get; set; }

        public string? BannerImagePath { get; set; }

        [StringLength(80)]
        public string? Country { get; set; }

        public bool IsVerified { get; set; }

        [StringLength(200)]
        public string? WebsiteUrl { get; set; }

        [StringLength(200)]
        public string? InstagramUrl { get; set; }

        [StringLength(200)]
        public string? TwitterUrl { get; set; }

        [StringLength(200)]
        public string? SpotifyUrl { get; set; }

        [StringLength(200)]
        public string? YouTubeUrl { get; set; }

        public Guid? UserId { get; set; }
        public User? User { get; set; }

        public ICollection<MusicTrack> Tracks { get; set; } = new List<MusicTrack>();
        public ICollection<Album> Albums { get; set; } = new List<Album>();
        public ICollection<Video> Videos { get; set; } = new List<Video>();
    }
}
