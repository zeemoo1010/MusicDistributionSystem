using MusicDistributionSystem.Application.DTOs.Common;

namespace MusicDistributionSystem.Application.DTOs.Music
{
    public class MusicIndexDto
    {
        public IReadOnlyCollection<MusicCardDto> Tracks { get; set; } = Array.Empty<MusicCardDto>();
        public IReadOnlyCollection<CategoryOptionDto> Categories { get; set; } = Array.Empty<CategoryOptionDto>();
        public string? SearchTerm { get; set; }
        public Guid? CategoryId { get; set; }
    }
}

