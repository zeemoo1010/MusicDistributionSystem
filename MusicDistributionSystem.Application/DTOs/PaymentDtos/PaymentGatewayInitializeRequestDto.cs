namespace MusicDistributionSystem.Application.DTOs.Payment
{
    public class PaymentGatewayInitializeRequestDto
    {
        public string Email { get; set; } = string.Empty;

        public decimal Amount { get; set; }

        public string Currency { get; set; } = "NGN";

        public string Reference { get; set; } = string.Empty;

        public string CallbackUrl { get; set; } = string.Empty;

        public Guid UserId { get; set; }

        public Guid PlanId { get; set; }
    }
}
