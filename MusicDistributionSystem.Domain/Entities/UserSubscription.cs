using MusicDistributionSystem.Domain.Common;

namespace MusicDistributionSystem.Domain.Entities
{
    public class UserSubscription : BaseEntity
    {
        public Guid UserId { get; set; }
        public User User { get; set; } = null!;

        public Guid PlanId { get; set; }
        public MembershipPlan Plan { get; set; } = null!;

        public DateTime StartDateUtc { get; set; }
        public DateTime? EndDateUtc { get; set; }
        public bool IsActive { get; set; } = true;
        public bool AutoRenew { get; set; }
        public string? PaystackSubscriptionCode { get; set; }
    }
}