using FluentAssertions;
using Moq;
using MusicDistributionSystem.Application.Contracts.Infrastructure;
using MusicDistributionSystem.Application.DTOs.PlaylistDtos;
using MusicDistributionSystem.Application.Services;
using MusicDistributionSystem.Domain.Contracts.Interface;
using MusicDistributionSystem.Domain.Entities;

namespace MusicDistributionSystem.Tests.Services
{
    public class PlaylistServiceTests
    {
        private readonly Mock<IPlaylistRepository> _playlistRepo = new();
        private readonly Mock<IMusicRepository> _musicRepo = new();
        private readonly Mock<IFileStorageService> _fileStorage = new();
        private readonly PlaylistService _sut;

        public PlaylistServiceTests()
        {
            _sut = new PlaylistService(_playlistRepo.Object, _musicRepo.Object, _fileStorage.Object);
        }

        [Fact]
        public async Task GetPublicPlaylistsAsync_ShouldReturnPublicPlaylists()
        {
            var playlists = new List<Playlist>
            {
                new() { Title = "Afrobeats Workout", Slug = "afrobeats-workout", IsPublic = true, User = new User { Username = "Curator" } }
            };

            _playlistRepo.Setup(r => r.GetPublicPlaylistsAsync()).ReturnsAsync(playlists);

            var result = await _sut.GetPublicPlaylistsAsync();

            result.Should().HaveCount(1);
            result.First().Title.Should().Be("Afrobeats Workout");
            result.First().OwnerName.Should().Be("Curator");
        }

        [Fact]
        public async Task CreatePlaylistAsync_WhenValid_ShouldAddPlaylist()
        {
            var userId = Guid.NewGuid();
            var request = new CreatePlaylistRequestDto
            {
                Title = "Late Night Chill",
                IsPublic = true
            };

            _playlistRepo.Setup(r => r.GetBySlugWithTracksAsync(It.IsAny<string>())).ReturnsAsync((Playlist?)null);

            var result = await _sut.CreatePlaylistAsync(request, userId);

            result.Succeeded.Should().BeTrue();
            _playlistRepo.Verify(r => r.AddAsync(It.Is<Playlist>(p => p.Title == "Late Night Chill")), Times.Once);
            _playlistRepo.Verify(r => r.SaveChangesAsync(), Times.Once);
        }
    }
}
