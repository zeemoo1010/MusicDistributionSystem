using MusicDistributionSystem.Domain.Common;

namespace MusicDistributionSystem.Domain.Entities
{
    public class Like : BaseEntity
    {
        public Guid UserId { get; set; }
        public User User { get; set; } = null!;

        public Guid? MusicTrackId { get; set; }
        public MusicTrack? MusicTrack { get; set; }

        public Guid? VideoId { get; set; }
        public Video? Video { get; set; }
    }
}