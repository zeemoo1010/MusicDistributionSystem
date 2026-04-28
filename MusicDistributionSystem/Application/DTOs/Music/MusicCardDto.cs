using MusicDistributionSystem.Domain.Enums;

namespace MusicDistributionSystem.Application.DTOs.Music
{
    public class MusicCardDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Artist { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? CategoryName { get; set; }
        public ContentAccessLevel AccessLevel { get; set; }
        public int DownloadCount { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsFeatured { get; set; }
    }
}

