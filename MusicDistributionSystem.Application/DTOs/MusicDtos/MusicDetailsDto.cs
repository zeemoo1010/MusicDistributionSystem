using MusicDistributionSystem.Domain.Enums;

namespace MusicDistributionSystem.Application.DTOs.Music
{
    public class MusicDetailsDto
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
        public Guid? CategoryId { get; set; }
        public string? CoverImagePath { get; set; }
        public string? Duration { get; set; }
        public string UploadedByName { get; set; } = string.Empty;
        public ContentAccessLevel AccessLevel { get; set; }
        public int DownloadCount { get; set; }
        public int PlayCount { get; set; }
        public int LikeCount { get; set; }
        public long FileSizeBytes { get; set; }
        public DateTime CreatedAt { get; set; }
        public IReadOnlyCollection<MusicCommentDto> Comments { get; set; } = Array.Empty<MusicCommentDto>();
    }

    public class MusicCommentDto
    {
        public Guid Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}


