using MusicDistributionSystem.Domain.Enums;

namespace MusicDistributionSystem.Application.DTOs.AlbumDtos
{
    public class AlbumCardDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public string ArtistName { get; set; } = string.Empty;
        public string? CategoryName { get; set; }
        public string? CoverImagePath { get; set; }
        public DateTime? ReleaseDate { get; set; }
        public ContentAccessLevel AccessLevel { get; set; }
        public int TrackCount { get; set; }
        public bool IsFeatured { get; set; }
    }

    public class AlbumDetailsDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public Guid ArtistId { get; set; }
        public string ArtistName { get; set; } = string.Empty;
        public string? CategoryName { get; set; }
        public string? CoverImagePath { get; set; }
        public string? Description { get; set; }
        public DateTime? ReleaseDate { get; set; }
        public ContentAccessLevel AccessLevel { get; set; }
        public DateTime CreatedAt { get; set; }
        public IReadOnlyCollection<MusicDistributionSystem.Application.DTOs.Music.MusicCardDto> Tracks { get; set; } = Array.Empty<MusicDistributionSystem.Application.DTOs.Music.MusicCardDto>();
    }

    public class CreateAlbumRequestDto
    {
        public string Title { get; set; } = string.Empty;
        public Guid ArtistId { get; set; }
        public Guid CategoryId { get; set; }
        public string? Description { get; set; }
        public DateTime? ReleaseDate { get; set; }
        public ContentAccessLevel AccessLevel { get; set; } = ContentAccessLevel.Free;
        public Microsoft.AspNetCore.Http.IFormFile? CoverImage { get; set; }
    }
}
