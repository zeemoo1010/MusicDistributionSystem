namespace MusicDistributionSystem.Application.DTOs.ArtistDtos
{
    public class ArtistCardDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public string? Bio { get; set; }
        public string? ProfilePicturePath { get; set; }
        public string? Country { get; set; }
        public bool IsVerified { get; set; }
        public int TrackCount { get; set; }
        public int AlbumCount { get; set; }
        public int VideoCount { get; set; }
    }

    public class ArtistDetailsDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public string? Bio { get; set; }
        public string? ProfilePicturePath { get; set; }
        public string? BannerImagePath { get; set; }
        public string? Country { get; set; }
        public bool IsVerified { get; set; }
        public string? WebsiteUrl { get; set; }
        public string? InstagramUrl { get; set; }
        public string? TwitterUrl { get; set; }
        public string? SpotifyUrl { get; set; }
        public string? YouTubeUrl { get; set; }
        public IReadOnlyCollection<MusicDistributionSystem.Application.DTOs.Music.MusicCardDto> Tracks { get; set; } = Array.Empty<MusicDistributionSystem.Application.DTOs.Music.MusicCardDto>();
        public IReadOnlyCollection<MusicDistributionSystem.Application.DTOs.AlbumDtos.AlbumCardDto> Albums { get; set; } = Array.Empty<MusicDistributionSystem.Application.DTOs.AlbumDtos.AlbumCardDto>();
        public IReadOnlyCollection<MusicDistributionSystem.Application.DTOs.VideoDtos.VideoCardDto> Videos { get; set; } = Array.Empty<MusicDistributionSystem.Application.DTOs.VideoDtos.VideoCardDto>();
    }

    public class CreateArtistRequestDto
    {
        public string Name { get; set; } = string.Empty;
        public string? Bio { get; set; }
        public string? Country { get; set; }
        public string? WebsiteUrl { get; set; }
        public string? InstagramUrl { get; set; }
        public string? TwitterUrl { get; set; }
        public string? SpotifyUrl { get; set; }
        public string? YouTubeUrl { get; set; }
        public Microsoft.AspNetCore.Http.IFormFile? ProfilePicture { get; set; }
        public Microsoft.AspNetCore.Http.IFormFile? BannerImage { get; set; }
    }
}
