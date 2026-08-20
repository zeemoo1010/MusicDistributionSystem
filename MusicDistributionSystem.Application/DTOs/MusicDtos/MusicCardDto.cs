using MusicDistributionSystem.Domain.Enums;

namespace MusicDistributionSystem.Application.DTOs.Music
{
    public class MusicCardDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Artist { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public Guid? ArtistId { get; set; }
        public Guid? AlbumId { get; set; }
        public string? AlbumTitle { get; set; }
        public string? Description { get; set; }
        public string? CategoryName { get; set; }
        public string? CoverImagePath { get; set; }
        public string? Duration { get; set; }
        public ContentAccessLevel AccessLevel { get; set; }
        public int DownloadCount { get; set; }
        public int PlayCount { get; set; }
        public int LikeCount { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsFeatured { get; set; }
    }
}


