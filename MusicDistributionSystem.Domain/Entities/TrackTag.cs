namespace MusicDistributionSystem.Domain.Entities
{
    public class TrackTag
    {
        public Guid MusicTrackId { get; set; }
        public MusicTrack MusicTrack { get; set; } = null!;

        public Guid TagId { get; set; }
        public Tag Tag { get; set; } = null!;
    }
}
