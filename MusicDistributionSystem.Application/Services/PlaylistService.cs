using MusicDistributionSystem.Application.Contracts.Infrastructure;
using MusicDistributionSystem.Application.Contracts.Services;
using MusicDistributionSystem.Application.DTOs.Account;
using MusicDistributionSystem.Application.DTOs.Music;
using MusicDistributionSystem.Application.DTOs.PlaylistDtos;
using MusicDistributionSystem.Domain.Common;
using MusicDistributionSystem.Domain.Contracts.Interface;
using MusicDistributionSystem.Domain.Entities;

namespace MusicDistributionSystem.Application.Services
{
    public class PlaylistService : IPlaylistService
    {
        private readonly IPlaylistRepository _playlistRepository;
        private readonly IMusicRepository _musicRepository;
        private readonly IFileStorageService _fileStorageService;

        public PlaylistService(
            IPlaylistRepository playlistRepository,
            IMusicRepository musicRepository,
            IFileStorageService fileStorageService)
        {
            _playlistRepository = playlistRepository;
            _musicRepository = musicRepository;
            _fileStorageService = fileStorageService;
        }

        public async Task<IReadOnlyCollection<PlaylistCardDto>> GetPublicPlaylistsAsync()
        {
            var lists = await _playlistRepository.GetPublicPlaylistsAsync();
            return lists.Select(MapPlaylistCard).ToList();
        }

        public async Task<IReadOnlyCollection<PlaylistCardDto>> GetUserPlaylistsAsync(Guid userId)
        {
            var lists = await _playlistRepository.GetUserPlaylistsAsync(userId);
            return lists.Select(MapPlaylistCard).ToList();
        }

        public async Task<PlaylistDetailsDto?> GetPlaylistBySlugAsync(string slug)
        {
            var playlist = await _playlistRepository.GetBySlugWithTracksAsync(slug);
            if (playlist is null) return null;
            return MapPlaylistDetails(playlist);
        }

        public async Task<PlaylistDetailsDto?> GetPlaylistByIdAsync(Guid id)
        {
            var playlist = await _playlistRepository.GetByIdWithTracksAsync(id);
            if (playlist is null) return null;
            return MapPlaylistDetails(playlist);
        }

        public async Task<OperationResultDto> CreatePlaylistAsync(CreatePlaylistRequestDto request, Guid userId)
        {
            if (string.IsNullOrWhiteSpace(request.Title))
            {
                return new OperationResultDto { ErrorMessage = "Playlist title is required." };
            }

            var baseSlug = SlugHelper.GenerateSlug(request.Title);
            var uniqueSlug = $"{baseSlug}-{Guid.NewGuid().ToString("N")[..6]}";

            string? coverPath = null;
            if (request.CoverImage is not null)
            {
                coverPath = await _fileStorageService.SaveFileAsync(request.CoverImage, "playlists", $"pl-{uniqueSlug}.jpg");
            }

            var playlist = new Playlist
            {
                Title = request.Title.Trim(),
                Slug = uniqueSlug,
                Description = request.Description?.Trim(),
                CoverImagePath = coverPath,
                IsPublic = request.IsPublic,
                UserId = userId
            };

            await _playlistRepository.AddAsync(playlist);
            await _playlistRepository.SaveChangesAsync();

            return new OperationResultDto { Succeeded = true };
        }

        public async Task<OperationResultDto> AddTrackAsync(Guid playlistId, Guid trackId, Guid userId)
        {
            var playlist = await _playlistRepository.GetByIdAsync(playlistId);
            if (playlist is null || playlist.UserId != userId)
            {
                return new OperationResultDto { ErrorMessage = "Playlist not found or permission denied." };
            }

            var track = await _musicRepository.GetApprovedTrackByIdAsync(trackId);
            if (track is null)
            {
                return new OperationResultDto { ErrorMessage = "Track not found." };
            }

            var playlistTrack = new PlaylistTrack
            {
                PlaylistId = playlistId,
                MusicTrackId = trackId,
                AddedAtUtc = DateTime.UtcNow
            };

            await _playlistRepository.AddTrackToPlaylistAsync(playlistTrack);
            await _playlistRepository.SaveChangesAsync();

            return new OperationResultDto { Succeeded = true };
        }

        public async Task<OperationResultDto> RemoveTrackAsync(Guid playlistId, Guid trackId, Guid userId)
        {
            var playlist = await _playlistRepository.GetByIdAsync(playlistId);
            if (playlist is null || playlist.UserId != userId)
            {
                return new OperationResultDto { ErrorMessage = "Playlist not found or permission denied." };
            }

            await _playlistRepository.RemoveTrackFromPlaylistAsync(playlistId, trackId);
            await _playlistRepository.SaveChangesAsync();

            return new OperationResultDto { Succeeded = true };
        }

        public async Task<OperationResultDto> DeletePlaylistAsync(Guid playlistId, Guid userId)
        {
            var playlist = await _playlistRepository.GetByIdAsync(playlistId);
            if (playlist is null || playlist.UserId != userId)
            {
                return new OperationResultDto { ErrorMessage = "Playlist not found or permission denied." };
            }

            _playlistRepository.Remove(playlist);
            await _playlistRepository.SaveChangesAsync();

            return new OperationResultDto { Succeeded = true };
        }

        private static PlaylistCardDto MapPlaylistCard(Playlist p) => new()
        {
            Id = p.Id,
            Title = p.Title,
            Slug = p.Slug,
            Description = p.Description,
            CoverImagePath = p.CoverImagePath,
            OwnerName = p.User?.Username ?? "SoundSphere Editorial",
            IsPublic = p.IsPublic,
            IsEditorial = p.IsEditorial,
            TrackCount = p.PlaylistTracks.Count,
            CreatedAt = p.CreatedAt
        };

        private static PlaylistDetailsDto MapPlaylistDetails(Playlist p) => new()
        {
            Id = p.Id,
            Title = p.Title,
            Slug = p.Slug,
            Description = p.Description,
            CoverImagePath = p.CoverImagePath,
            UserId = p.UserId,
            OwnerName = p.User?.Username ?? "SoundSphere Editorial",
            IsPublic = p.IsPublic,
            IsEditorial = p.IsEditorial,
            CreatedAt = p.CreatedAt,
            Tracks = p.PlaylistTracks.Select(pt => new MusicCardDto
            {
                Id = pt.MusicTrack.Id,
                Title = pt.MusicTrack.Title,
                Artist = pt.MusicTrack.Artist,
                Slug = pt.MusicTrack.Slug,
                CategoryName = pt.MusicTrack.Category?.Name,
                CoverImagePath = pt.MusicTrack.CoverImagePath,
                Duration = pt.MusicTrack.Duration,
                AccessLevel = pt.MusicTrack.AccessLevel,
                DownloadCount = pt.MusicTrack.DownloadCount,
                PlayCount = pt.MusicTrack.PlayCount,
                CreatedAt = pt.MusicTrack.CreatedAt
            }).ToList()
        };
    }
}
