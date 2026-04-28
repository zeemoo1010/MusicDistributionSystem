using MusicDistributionSystem.Domain.Enums;

namespace MusicDistributionSystem.Application.DTOs.Membership
{
    public class MembershipPlanDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public MembershipTier Tier { get; set; }
        public decimal MonthlyPrice { get; set; }
        public int MonthlyDownloadLimit { get; set; }
        public bool HasAdFreeExperience { get; set; }
        public bool HasPriorityReview { get; set; }
        public bool HasArtistPromotionTools { get; set; }
        public string Description { get; set; } = string.Empty;
    }
}

