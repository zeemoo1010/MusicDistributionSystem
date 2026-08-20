namespace MusicDistributionSystem.Application.DTOs.PlaylistDtos
{
    public class PlaylistCardDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? CoverImagePath { get; set; }
        public string OwnerName { get; set; } = string.Empty;
        public bool IsPublic { get; set; }
        public bool IsEditorial { get; set; }
        public int TrackCount { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class PlaylistDetailsDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? CoverImagePath { get; set; }
        public Guid UserId { get; set; }
        public string OwnerName { get; set; } = string.Empty;
        public bool IsPublic { get; set; }
        public bool IsEditorial { get; set; }
        public DateTime CreatedAt { get; set; }
        public IReadOnlyCollection<MusicDistributionSystem.Application.DTOs.Music.MusicCardDto> Tracks { get; set; } = Array.Empty<MusicDistributionSystem.Application.DTOs.Music.MusicCardDto>();
    }

    public class CreatePlaylistRequestDto
    {
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsPublic { get; set; } = true;
        public Microsoft.AspNetCore.Http.IFormFile? CoverImage { get; set; }
    }
}
