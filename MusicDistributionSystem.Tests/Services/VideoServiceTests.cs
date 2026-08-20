using FluentAssertions;
using Moq;
using MusicDistributionSystem.Application.Contracts.Infrastructure;
using MusicDistributionSystem.Application.Contracts.Security;
using MusicDistributionSystem.Application.Services;
using MusicDistributionSystem.Domain.Common;
using MusicDistributionSystem.Domain.Contracts.Interface;
using MusicDistributionSystem.Domain.Entities;
using MusicDistributionSystem.Domain.Enums;

namespace MusicDistributionSystem.Tests.Services
{
    public class VideoServiceTests
    {
        private readonly Mock<IVideoRepository> _videoRepo = new();
        private readonly Mock<ICategoryRepository> _categoryRepo = new();
        private readonly Mock<IArtistRepository> _artistRepo = new();
        private readonly Mock<IFileStorageService> _fileStorage = new();
        private readonly Mock<IUploadedFileSecurityService> _fileSecurity = new();
        private readonly Mock<IUserRepository> _userRepo = new();
        private readonly VideoService _sut;

        public VideoServiceTests()
        {
            _sut = new VideoService(
                _videoRepo.Object,
                _categoryRepo.Object,
                _artistRepo.Object,
                _fileStorage.Object,
                _fileSecurity.Object,
                _userRepo.Object);
        }

        [Fact]
        public async Task GetVideoIndexAsync_ShouldReturnPagedVideos()
        {
            var paged = new PaginatedResult<Video>
            {
                Items = new[] { new Video { Title = "Live Concert", Slug = "live-concert", AccessLevel = ContentAccessLevel.Free } },
                TotalCount = 1,
                Page = 1,
                PageSize = 12
            };

            _videoRepo.Setup(r => r.GetApprovedVideosPagedAsync(null, null, 1, 12)).ReturnsAsync(paged);
            _categoryRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(Array.Empty<Category>());

            var result = await _sut.GetVideoIndexAsync(null, null, 1, 12);

            result.Videos.Should().HaveCount(1);
            result.Videos.First().Title.Should().Be("Live Concert");
        }

        [Fact]
        public async Task PrepareDownloadAsync_WhenVideoNotFound_ShouldReturnFoundFalse()
        {
            _videoRepo.Setup(r => r.GetApprovedVideoByIdAsync(It.IsAny<Guid>(), false)).ReturnsAsync((Video?)null);

            var result = await _sut.PrepareDownloadAsync(Guid.NewGuid(), null, null);

            result.Found.Should().BeFalse();
        }
    }
}
