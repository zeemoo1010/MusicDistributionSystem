using MusicDistributionSystem.Domain.Common;

namespace MusicDistributionSystem.Domain.Entities
{
    public class Like : BaseEntity
    {
        public Guid UserId { get; set; }
        public User User { get; set; } = null!;

        public Guid MediaAssetId { get; set; }
        public MediaAsset MediaAsset { get; set; } = null!;
    }
}