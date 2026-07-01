namespace MusicDistributionSystem.Application.DTOs.Payment
{
    public class PaymentVerificationResultDto
    {
        public bool Succeeded { get; set; }

        public string? PlanName { get; set; }

        public string? ErrorMessage { get; set; }
    }
}
