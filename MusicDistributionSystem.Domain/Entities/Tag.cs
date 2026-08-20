using MusicDistributionSystem.Domain.Common;

namespace MusicDistributionSystem.Domain.Entities
{
    public class Tag : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public string? Slug { get; set; }
        public ICollection<TrackTag> TrackTags { get; set; } = new List<TrackTag>();
    }
}