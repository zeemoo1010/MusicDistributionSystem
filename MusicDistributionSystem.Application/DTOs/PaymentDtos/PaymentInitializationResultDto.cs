namespace MusicDistributionSystem.Application.DTOs.Payment
{
    public class PaymentInitializationResultDto
    {
        public bool Succeeded { get; set; }

        public bool RequiresRedirect { get; set; }

        public string? AuthorizationUrl { get; set; }

        public string? ErrorMessage { get; set; }
    }
}
