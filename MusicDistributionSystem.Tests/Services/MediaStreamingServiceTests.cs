using FluentAssertions;
using Moq;
using MusicDistributionSystem.Application.Contracts.Infrastructure;
using MusicDistributionSystem.Application.Services;
using MusicDistributionSystem.Domain.Contracts.Interface;
using MusicDistributionSystem.Domain.Entities;
using MusicDistributionSystem.Domain.Enums;

namespace MusicDistributionSystem.Tests.Services
{
    public class MediaStreamingServiceTests
    {
        private readonly Mock<IMusicRepository> _musicRepo = new();
        private readonly Mock<IVideoRepository> _videoRepo = new();
        private readonly Mock<IUserRepository> _userRepo = new();
        private readonly Mock<IFileStorageService> _fileStorage = new();
        private readonly MediaStreamingService _sut;

        public MediaStreamingServiceTests()
        {
            _sut = new MediaStreamingService(
                _musicRepo.Object,
                _videoRepo.Object,
                _userRepo.Object,
                _fileStorage.Object);
        }

        [Fact]
        public async Task GetMediaStreamAsync_WhenTrackNotFound_ShouldReturnFoundFalse()
        {
            _musicRepo.Setup(r => r.GetApprovedTrackByIdAsync(It.IsAny<Guid>(), It.IsAny<bool>()))
                .ReturnsAsync((MusicTrack?)null);

            var (found, allowed, path, type, len, err) = await _sut.GetMediaStreamAsync(Guid.NewGuid(), "music", null);

            found.Should().BeFalse();
            allowed.Should().BeFalse();
        }

        [Fact]
        public async Task GetMediaStreamAsync_WhenFreeTrackExists_ShouldAllowStreaming()
        {
            var tempFile = Path.GetTempFileName();
            try
            {
                File.WriteAllText(tempFile, "dummy audio bytes");

                var track = new MusicTrack
                {
                    Title = "Afrobeats Anthem",
                    AccessLevel = ContentAccessLevel.Free,
                    FilePath = "uploads/music/track.mp3"
                };

                _musicRepo.Setup(r => r.GetApprovedTrackByIdAsync(track.Id, It.IsAny<bool>()))
                    .ReturnsAsync(track);
                _fileStorage.Setup(s => s.FileExists(track.FilePath)).Returns(true);
                _fileStorage.Setup(s => s.GetPhysicalPath(track.FilePath)).Returns(tempFile);

                var (found, allowed, path, type, len, err) = await _sut.GetMediaStreamAsync(track.Id, "music", null);

                found.Should().BeTrue();
                allowed.Should().BeTrue();
                path.Should().Be(tempFile);
                type.Should().Be("audio/mpeg");
            }
            finally
            {
                if (File.Exists(tempFile)) File.Delete(tempFile);
            }
        }

        [Fact]
        public async Task GetMediaStreamAsync_WhenPremiumTrack_AndUserIsFree_ShouldBlock()
        {
            var track = new MusicTrack
            {
                Title = "VIP Song",
                AccessLevel = ContentAccessLevel.Premium,
                FilePath = "uploads/music/vip.mp3"
            };

            var userId = Guid.NewGuid();
            var user = new User
            {
                Username = "freeuser",
                MembershipTier = MembershipTier.Free
            };

            _musicRepo.Setup(r => r.GetApprovedTrackByIdAsync(track.Id, It.IsAny<bool>())).ReturnsAsync(track);
            _userRepo.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync(user);

            var (found, allowed, path, type, len, err) = await _sut.GetMediaStreamAsync(track.Id, "music", userId);

            found.Should().BeTrue();
            allowed.Should().BeFalse();
            err.Should().Contain("Upgrade");
        }

        [Fact]
        public async Task RecordPlayAsync_ShouldIncrementPlayCount()
        {
            var track = new MusicTrack { Title = "Song", PlayCount = 5 };
            _musicRepo.Setup(r => r.GetApprovedTrackByIdAsync(track.Id, It.IsAny<bool>())).ReturnsAsync(track);

            await _sut.RecordPlayAsync(track.Id, null, "127.0.0.1");

            track.PlayCount.Should().Be(6);
            _musicRepo.Verify(r => r.SaveChangesAsync(), Times.Once);
        }
    }
}
