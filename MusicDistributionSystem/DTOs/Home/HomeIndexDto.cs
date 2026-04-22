using MusicDistributionSystem.DTOs.Common;
using MusicDistributionSystem.DTOs.Membership;
using MusicDistributionSystem.DTOs.Music;

namespace MusicDistributionSystem.DTOs.Home
{
    public class HomeIndexDto
    {
        public IReadOnlyCollection<MusicCardDto> LatestTracks { get; set; } = Array.Empty<MusicCardDto>();
        public IReadOnlyCollection<CategoryOptionDto> Categories { get; set; } = Array.Empty<CategoryOptionDto>();
        public IReadOnlyCollection<MembershipPlanDto> MembershipPlans { get; set; } = Array.Empty<MembershipPlanDto>();
        public int ApprovedTrackCount { get; set; }
        public int TotalDownloads { get; set; }
        public int PremiumTrackCount { get; set; }
    }
}
