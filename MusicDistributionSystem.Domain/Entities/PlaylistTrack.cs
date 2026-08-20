namespace MusicDistributionSystem.Domain.Entities
{
    public class PlaylistTrack
    {
        public Guid PlaylistId { get; set; }
        public Playlist Playlist { get; set; } = null!;

        public Guid MusicTrackId { get; set; }
        public MusicTrack MusicTrack { get; set; } = null!;

        public int OrderIndex { get; set; }

        public DateTime AddedAtUtc { get; set; } = DateTime.UtcNow;
    }
}
