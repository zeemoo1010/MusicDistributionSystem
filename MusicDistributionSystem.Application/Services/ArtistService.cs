using MusicDistributionSystem.Application.Contracts.Infrastructure;
using MusicDistributionSystem.Application.Contracts.Services;
using MusicDistributionSystem.Application.DTOs.Account;
using MusicDistributionSystem.Application.DTOs.AlbumDtos;
using MusicDistributionSystem.Application.DTOs.ArtistDtos;
using MusicDistributionSystem.Application.DTOs.Music;
using MusicDistributionSystem.Application.DTOs.VideoDtos;
using MusicDistributionSystem.Domain.Common;
using MusicDistributionSystem.Domain.Contracts.Interface;
using MusicDistributionSystem.Domain.Entities;

namespace MusicDistributionSystem.Application.Services
{
    public class ArtistService : IArtistService
    {
        private readonly IArtistRepository _artistRepository;
        private readonly IFileStorageService _fileStorageService;

        public ArtistService(IArtistRepository artistRepository, IFileStorageService fileStorageService)
        {
            _artistRepository = artistRepository;
            _fileStorageService = fileStorageService;
        }

        public async Task<IReadOnlyCollection<ArtistCardDto>> GetFeaturedArtistsAsync(int take = 8)
        {
            var artists = await _artistRepository.GetFeaturedArtistsAsync(take);
            return artists.Select(MapArtistCard).ToList();
        }

        public async Task<IReadOnlyCollection<ArtistCardDto>> GetAllArtistsAsync()
        {
            var artists = await _artistRepository.GetAllAsync();
            return artists.Select(MapArtistCard).ToList();
        }

        public async Task<ArtistDetailsDto?> GetArtistBySlugAsync(string slug)
        {
            var artist = await _artistRepository.GetBySlugAsync(slug);
            if (artist is null) return null;
            return MapArtistDetails(artist);
        }

        public async Task<ArtistDetailsDto?> GetArtistByIdAsync(Guid id)
        {
            var artist = await _artistRepository.GetByIdAsync(id);
            if (artist is null) return null;
            return MapArtistDetails(artist);
        }

        public async Task<OperationResultDto> CreateArtistAsync(CreateArtistRequestDto request)
        {
            if (string.IsNullOrWhiteSpace(request.Name))
            {
                return new OperationResultDto { ErrorMessage = "Artist name is required." };
            }

            var slug = SlugHelper.GenerateSlug(request.Name);

            string? profilePicPath = null;
            if (request.ProfilePicture is not null)
            {
                profilePicPath = await _fileStorageService.SaveFileAsync(request.ProfilePicture, "artists", $"artist-{slug}-profile.jpg");
            }

            string? bannerPath = null;
            if (request.BannerImage is not null)
            {
                bannerPath = await _fileStorageService.SaveFileAsync(request.BannerImage, "artists", $"artist-{slug}-banner.jpg");
            }

            var artist = new Artist
            {
                Name = request.Name.Trim(),
                Slug = slug,
                Bio = request.Bio?.Trim(),
                Country = request.Country?.Trim(),
                ProfilePicturePath = profilePicPath,
                BannerImagePath = bannerPath,
                WebsiteUrl = request.WebsiteUrl?.Trim(),
                InstagramUrl = request.InstagramUrl?.Trim(),
                TwitterUrl = request.TwitterUrl?.Trim(),
                SpotifyUrl = request.SpotifyUrl?.Trim(),
                YouTubeUrl = request.YouTubeUrl?.Trim(),
                IsVerified = true
            };

            await _artistRepository.AddAsync(artist);
            await _artistRepository.SaveChangesAsync();

            return new OperationResultDto { Succeeded = true };
        }

        private static ArtistCardDto MapArtistCard(Artist a) => new()
        {
            Id = a.Id,
            Name = a.Name,
            Slug = a.Slug,
            Bio = a.Bio,
            ProfilePicturePath = a.ProfilePicturePath,
            Country = a.Country,
            IsVerified = a.IsVerified,
            TrackCount = a.Tracks.Count,
            AlbumCount = a.Albums.Count,
            VideoCount = a.Videos.Count
        };

        private static ArtistDetailsDto MapArtistDetails(Artist a) => new()
        {
            Id = a.Id,
            Name = a.Name,
            Slug = a.Slug,
            Bio = a.Bio,
            ProfilePicturePath = a.ProfilePicturePath,
            BannerImagePath = a.BannerImagePath,
            Country = a.Country,
            IsVerified = a.IsVerified,
            WebsiteUrl = a.WebsiteUrl,
            InstagramUrl = a.InstagramUrl,
            TwitterUrl = a.TwitterUrl,
            SpotifyUrl = a.SpotifyUrl,
            YouTubeUrl = a.YouTubeUrl,
            Tracks = a.Tracks.Select(t => new MusicCardDto
            {
                Id = t.Id,
                Title = t.Title,
                Artist = a.Name,
                Slug = t.Slug,
                ArtistId = a.Id,
                CategoryName = t.Category?.Name,
                CoverImagePath = t.CoverImagePath ?? a.ProfilePicturePath,
                AccessLevel = t.AccessLevel,
                DownloadCount = t.DownloadCount,
                PlayCount = t.PlayCount,
                CreatedAt = t.CreatedAt
            }).ToList(),
            Albums = a.Albums.Select(al => new AlbumCardDto
            {
                Id = al.Id,
                Title = al.Title,
                Slug = al.Slug,
                ArtistName = a.Name,
                CategoryName = al.Category?.Name,
                CoverImagePath = al.CoverImagePath,
                ReleaseDate = al.ReleaseDate,
                AccessLevel = al.AccessLevel,
                TrackCount = al.Tracks.Count
            }).ToList(),
            Videos = a.Videos.Select(v => new VideoCardDto
            {
                Id = v.Id,
                Title = v.Title,
                Slug = v.Slug,
                ArtistName = a.Name,
                CategoryName = v.Category?.Name,
                ThumbnailPath = v.ThumbnailPath,
                Duration = v.Duration,
                ViewCount = v.ViewCount,
                DownloadCount = v.DownloadCount,
                AccessLevel = v.AccessLevel,
                CreatedAt = v.CreatedAt
            }).ToList()
        };
    }
}
