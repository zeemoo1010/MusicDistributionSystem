using MusicDistributionSystem.Domain.Common;
using System.ComponentModel.DataAnnotations;

namespace MusicDistributionSystem.Domain.Entities
{
    public class Playlist : BaseEntity
    {
        [Required]
        [StringLength(150)]
        public string Title { get; set; } = string.Empty;

        [StringLength(180)]
        public string Slug { get; set; } = string.Empty;

        [StringLength(1000)]
        public string? Description { get; set; }

        public string? CoverImagePath { get; set; }

        public bool IsPublic { get; set; } = true;

        public bool IsEditorial { get; set; }

        public Guid UserId { get; set; }
        public User? User { get; set; }

        public ICollection<PlaylistTrack> PlaylistTracks { get; set; } = new List<PlaylistTrack>();
    }
}
