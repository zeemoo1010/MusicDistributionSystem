using MusicDistributionSystem.Domain.Common;

namespace MusicDistributionSystem.Domain.Entities
{
    public class Tag : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public string? Slug { get; set; }
        public ICollection<MediaAssetTag> MediaAssetTags { get; set; } = new List<MediaAssetTag>();
    }
}