using MusicDistributionSystem.Models;

namespace MusicDistributionSystem.ViewModels
{
    public class MusicIndexViewModel
    {
        public IReadOnlyCollection<MusicTrack> Tracks { get; set; } = Array.Empty<MusicTrack>();
        public IReadOnlyCollection<Category> Categories { get; set; } = Array.Empty<Category>();
        public string? SearchTerm { get; set; }
        public Guid? CategoryId { get; set; }
    }
}
