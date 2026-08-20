using MusicDistributionSystem.Application.Contracts.Infrastructure;
using MusicDistributionSystem.Application.Contracts.Services;
using MusicDistributionSystem.Application.DTOs.Account;
using MusicDistributionSystem.Application.DTOs.AlbumDtos;
using MusicDistributionSystem.Application.DTOs.Music;
using MusicDistributionSystem.Domain.Common;
using MusicDistributionSystem.Domain.Contracts.Interface;
using MusicDistributionSystem.Domain.Entities;
using MusicDistributionSystem.Domain.Enums;

namespace MusicDistributionSystem.Application.Services
{
    public class AlbumService : IAlbumService
    {
        private readonly IAlbumRepository _albumRepository;
        private readonly IArtistRepository _artistRepository;
        private readonly IFileStorageService _fileStorageService;

        public AlbumService(
            IAlbumRepository albumRepository,
            IArtistRepository artistRepository,
            IFileStorageService fileStorageService)
        {
            _albumRepository = albumRepository;
            _artistRepository = artistRepository;
            _fileStorageService = fileStorageService;
        }

        public async Task<IReadOnlyCollection<AlbumCardDto>> GetFeaturedAlbumsAsync(int take = 6)
        {
            var albums = await _albumRepository.GetFeaturedAlbumsAsync(take);
            return albums.Select(MapAlbumCard).ToList();
        }

        public async Task<IReadOnlyCollection<AlbumCardDto>> GetAllAlbumsAsync()
        {
            var albums = await _albumRepository.GetAllAsync();
            return albums.Select(MapAlbumCard).ToList();
        }

        public async Task<AlbumDetailsDto?> GetAlbumBySlugAsync(string slug)
        {
            var album = await _albumRepository.GetBySlugAsync(slug);
            if (album is null) return null;
            return MapAlbumDetails(album);
        }

        public async Task<AlbumDetailsDto?> GetAlbumByIdAsync(Guid id)
        {
            var album = await _albumRepository.GetByIdAsync(id);
            if (album is null) return null;
            return MapAlbumDetails(album);
        }

        public async Task<OperationResultDto> CreateAlbumAsync(CreateAlbumRequestDto request, Guid uploadedByUserId)
        {
            if (string.IsNullOrWhiteSpace(request.Title))
            {
                return new OperationResultDto { ErrorMessage = "Album title is required." };
            }

            var artist = await _artistRepository.GetByIdAsync(request.ArtistId);
            if (artist is null)
            {
                return new OperationResultDto { ErrorMessage = "Selected artist does not exist." };
            }

            var slug = SlugHelper.GenerateSlug($"{request.Title}-{artist.Name}");

            string? coverPath = null;
            if (request.CoverImage is not null)
            {
                coverPath = await _fileStorageService.SaveFileAsync(request.CoverImage, "covers", $"album-{slug}.jpg");
            }

            var album = new Album
            {
                Title = request.Title.Trim(),
                Slug = slug,
                ArtistId = request.ArtistId,
                CategoryId = request.CategoryId,
                Description = request.Description?.Trim(),
                ReleaseDate = request.ReleaseDate ?? DateTime.UtcNow,
                AccessLevel = request.AccessLevel,
                CoverImagePath = coverPath,
                UploadedByUserId = uploadedByUserId,
                ApprovalStatus = ApprovalStatus.Approved,
                IsFeatured = true
            };

            await _albumRepository.AddAsync(album);
            await _albumRepository.SaveChangesAsync();

            return new OperationResultDto { Succeeded = true };
        }

        private static AlbumCardDto MapAlbumCard(Album a) => new()
        {
            Id = a.Id,
            Title = a.Title,
            Slug = a.Slug,
            ArtistName = a.Artist?.Name ?? "Various Artists",
            CategoryName = a.Category?.Name,
            CoverImagePath = a.CoverImagePath,
            ReleaseDate = a.ReleaseDate,
            AccessLevel = a.AccessLevel,
            TrackCount = a.Tracks.Count,
            IsFeatured = a.IsFeatured
        };

        private static AlbumDetailsDto MapAlbumDetails(Album a) => new()
        {
            Id = a.Id,
            Title = a.Title,
            Slug = a.Slug,
            ArtistId = a.ArtistId,
            ArtistName = a.Artist?.Name ?? "Various Artists",
            CategoryName = a.Category?.Name,
            CoverImagePath = a.CoverImagePath,
            Description = a.Description,
            ReleaseDate = a.ReleaseDate,
            AccessLevel = a.AccessLevel,
            CreatedAt = a.CreatedAt,
            Tracks = a.Tracks.Select(t => new MusicCardDto
            {
                Id = t.Id,
                Title = t.Title,
                Artist = a.Artist?.Name ?? t.Artist,
                Slug = t.Slug,
                ArtistId = a.ArtistId,
                AlbumId = a.Id,
                AlbumTitle = a.Title,
                CategoryName = a.Category?.Name,
                CoverImagePath = t.CoverImagePath ?? a.CoverImagePath,
                Duration = t.Duration,
                AccessLevel = t.AccessLevel,
                DownloadCount = t.DownloadCount,
                PlayCount = t.PlayCount,
                CreatedAt = t.CreatedAt
            }).ToList()
        };
    }
}
