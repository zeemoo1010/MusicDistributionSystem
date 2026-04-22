namespace MusicDistributionSystem.Models
{
    public class DownloadRecord
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid MusicTrackId { get; set; }
        public MusicTrack? MusicTrack { get; set; }

        public string? DownloaderIpAddress { get; set; }

        public DateTime DownloadedAt { get; set; } = DateTime.UtcNow;
    }
}
