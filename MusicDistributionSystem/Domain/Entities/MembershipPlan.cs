using System.ComponentModel.DataAnnotations;
using MusicDistributionSystem.Domain.Enums;

namespace MusicDistributionSystem.Domain.Entities
{
    public class MembershipPlan
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [StringLength(80)]
        public string Name { get; set; } = string.Empty;

        public MembershipTier Tier { get; set; }

        [Range(0, 1000000)]
        public decimal MonthlyPrice { get; set; }

        [Range(0, 1000000)]
        public int MonthlyDownloadLimit { get; set; }

        public bool HasAdFreeExperience { get; set; }

        public bool HasPriorityReview { get; set; }

        public bool HasArtistPromotionTools { get; set; }

        [StringLength(400)]
        public string Description { get; set; } = string.Empty;
    }
}

