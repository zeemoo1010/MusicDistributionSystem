using MusicDistributionSystem.Domain.Common;
using MusicDistributionSystem.Domain.Enums;

namespace MusicDistributionSystem.Domain.Entities
{
    public class PaymentTransaction : BaseEntity
    {
        public Guid UserId { get; set; }
        public User User { get; set; } = null!;

        public Guid? PlanId { get; set; }
        public MembershipPlan? Plan { get; set; }

        public decimal Amount { get; set; }
        public string Currency { get; set; } = "NGN";
        public PaymentStatus Status { get; set; } = PaymentStatus.Pending;
        public string Reference { get; set; } = string.Empty;
        public string? PaystackResponse { get; set; }
        public DateTime? PaidAtUtc { get; set; }
    }
}