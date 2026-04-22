namespace MusicDistributionSystem.Models
{
    public class DownloadRecord
    {
        public int Id { get; set; }

        public int MusicTrackId { get; set; }
        public MusicTrack? MusicTrack { get; set; }

        public string? DownloaderIpAddress { get; set; }

        public DateTime DownloadedAt { get; set; } = DateTime.UtcNow;
    }
}
