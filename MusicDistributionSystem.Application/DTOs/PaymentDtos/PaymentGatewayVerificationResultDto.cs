namespace MusicDistributionSystem.Application.DTOs.Payment
{
    public class PaymentGatewayVerificationResultDto
    {
        public bool Succeeded { get; set; }

        public string? Status { get; set; }

        public string? Reference { get; set; }

        public decimal Amount { get; set; }

        public string Currency { get; set; } = string.Empty;

        public Guid? UserId { get; set; }

        public Guid? PlanId { get; set; }

        public string? RawResponse { get; set; }

        public string? ErrorMessage { get; set; }
    }
}
