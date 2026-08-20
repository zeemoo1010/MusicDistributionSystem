using FluentAssertions;
using MusicDistributionSystem.Application.Services;

namespace MusicDistributionSystem.Tests.Services
{
    public class AiContentServiceTests
    {
        private readonly AiContentService _sut = new();

        [Fact]
        public async Task GenerateReleaseDescriptionAsync_ShouldReturnFormattedDescription()
        {
            var result = await _sut.GenerateReleaseDescriptionAsync("City Boys", "Burna Boy", "Afrobeats", "High energy summer anthem");

            result.Should().NotBeNullOrWhiteSpace();
            result.Should().Contain("City Boys");
            result.Should().Contain("Burna Boy");
        }

        [Fact]
        public async Task GenerateTagsAsync_ShouldReturnStandardAndContextualTags()
        {
            var tags = await _sut.GenerateTagsAsync("Last Last", "Burna Boy", "Afrobeats");

            tags.Should().NotBeEmpty();
            tags.Should().Contain("BurnaBoy");
            tags.Should().Contain("Afrobeats");
        }

        [Fact]
        public async Task ParseNaturalLanguageSearchAsync_ShouldExtractMoodAndGenre()
        {
            var (keywords, genre, mood) = await _sut.ParseNaturalLanguageSearchAsync("I want energetic party afrobeats for workout");

            genre.Should().Be("Afrobeats");
            mood.Should().Contain("Energetic");
        }
    }
}
