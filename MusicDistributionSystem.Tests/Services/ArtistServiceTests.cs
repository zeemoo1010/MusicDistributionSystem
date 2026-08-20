using FluentAssertions;
using Moq;
using MusicDistributionSystem.Application.Contracts.Infrastructure;
using MusicDistributionSystem.Application.DTOs.ArtistDtos;
using MusicDistributionSystem.Application.Services;
using MusicDistributionSystem.Domain.Contracts.Interface;
using MusicDistributionSystem.Domain.Entities;

namespace MusicDistributionSystem.Tests.Services
{
    public class ArtistServiceTests
    {
        private readonly Mock<IArtistRepository> _artistRepo = new();
        private readonly Mock<IFileStorageService> _fileStorage = new();
        private readonly ArtistService _sut;

        public ArtistServiceTests()
        {
            _sut = new ArtistService(_artistRepo.Object, _fileStorage.Object);
        }

        [Fact]
        public async Task GetAllArtistsAsync_ShouldReturnMappedArtists()
        {
            var artists = new List<Artist>
            {
                new() { Name = "Burna Boy", Slug = "burna-boy", IsVerified = true, Country = "Nigeria" }
            };

            _artistRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(artists);

            var result = await _sut.GetAllArtistsAsync();

            result.Should().HaveCount(1);
            result.First().Name.Should().Be("Burna Boy");
            result.First().IsVerified.Should().BeTrue();
        }

        [Fact]
        public async Task GetArtistBySlugAsync_WhenFound_ShouldReturnDetails()
        {
            var artist = new Artist
            {
                Name = "Wizkid",
                Slug = "wizkid",
                Country = "Nigeria",
                Bio = "Grammy-winning star"
            };

            _artistRepo.Setup(r => r.GetBySlugAsync("wizkid", true)).ReturnsAsync(artist);

            var result = await _sut.GetArtistBySlugAsync("wizkid");

            result.Should().NotBeNull();
            result!.Name.Should().Be("Wizkid");
            result.Bio.Should().Be("Grammy-winning star");
        }

        [Fact]
        public async Task CreateArtistAsync_WhenValid_ShouldCreateAndSave()
        {
            var request = new CreateArtistRequestDto
            {
                Name = "Asake",
                Country = "Nigeria",
                Bio = "Mr Money with the vibe"
            };

            _artistRepo.Setup(r => r.GetBySlugAsync(It.IsAny<string>(), false)).ReturnsAsync((Artist?)null);

            var result = await _sut.CreateArtistAsync(request);

            result.Succeeded.Should().BeTrue();
            _artistRepo.Verify(r => r.AddAsync(It.Is<Artist>(a => a.Name == "Asake")), Times.Once);
            _artistRepo.Verify(r => r.SaveChangesAsync(), Times.Once);
        }
    }
}
