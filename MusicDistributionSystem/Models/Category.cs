using System.ComponentModel.DataAnnotations;

namespace MusicDistributionSystem.Models
{
    public class Category
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [StringLength(80)]
        public string Name { get; set; } = string.Empty;

        [StringLength(250)]
        public string? Description { get; set; }

        public ICollection<MusicTrack> MusicTracks { get; set; } = new List<MusicTrack>();
    }
}
