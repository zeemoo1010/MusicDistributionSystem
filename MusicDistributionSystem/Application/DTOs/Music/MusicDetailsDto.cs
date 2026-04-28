using MusicDistributionSystem.Domain.Enums;

namespace MusicDistributionSystem.Application.DTOs.Music
{
    public class MusicDetailsDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Artist { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? CategoryName { get; set; }
        public string UploadedByName { get; set; } = string.Empty;
        public ContentAccessLevel AccessLevel { get; set; }
        public int DownloadCount { get; set; }
        public long FileSizeBytes { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}

