using MusicDistributionSystem.Models;

namespace MusicDistributionSystem.ViewModels
{
    public class HomeIndexViewModel
    {
        public IReadOnlyCollection<MusicTrack> LatestTracks { get; set; } = Array.Empty<MusicTrack>();
        public IReadOnlyCollection<Category> Categories { get; set; } = Array.Empty<Category>();
        public IReadOnlyCollection<MembershipPlan> MembershipPlans { get; set; } = Array.Empty<MembershipPlan>();
        public int ApprovedTrackCount { get; set; }
        public int TotalDownloads { get; set; }
        public int PremiumTrackCount { get; set; }
    }
}
