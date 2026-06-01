using MusicDistributionSystem.Domain.Enums;

namespace MusicDistributionSystem.Application.DTOs.Moderation
{
    public class ModerationTrackDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Artist { get; set; } = string.Empty;
        public string UploadedByName { get; set; } = string.Empty;
        public string UploadedByEmail { get; set; } = string.Empty;
        public string? CategoryName { get; set; }
        public string? Description { get; set; }
        public string? CoverImagePath { get; set; }
        public DateTime CreatedAt { get; set; }
        public ContentAccessLevel AccessLevel { get; set; }
    }
}

