using MusicDistributionSystem.Application.DTOs.Common;
using MusicDistributionSystem.Application.DTOs.Membership;
using MusicDistributionSystem.Application.DTOs.Music;

namespace MusicDistributionSystem.Application.DTOs.Home
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

