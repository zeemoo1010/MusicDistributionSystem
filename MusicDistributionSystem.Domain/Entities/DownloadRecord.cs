using MusicDistributionSystem.Domain.Common;

namespace MusicDistributionSystem.Domain.Entities
{
    public class DownloadRecord : BaseEntity
    {
        public Guid MusicTrackId { get; set; }
        public MusicTrack? MusicTrack { get; set; }

        public string? DownloaderIpAddress { get; set; }

        public DateTime DownloadedAt { get; set; } = DateTime.UtcNow;
    }
}

