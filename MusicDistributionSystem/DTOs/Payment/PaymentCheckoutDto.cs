namespace MusicDistributionSystem.DTOs.Payment
{
    public class PaymentCheckoutDto
    {
        public Guid PlanId { get; set; }
        public string PlanName { get; set; } = string.Empty;
        public string TierName { get; set; } = string.Empty;
        public decimal MonthlyPrice { get; set; }
        public string Description { get; set; } = string.Empty;
    }
}
