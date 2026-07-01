namespace MusicDistributionSystem.Application.DTOs.Payment
{
    public class PaymentGatewayInitializeResultDto
    {
        public bool Succeeded { get; set; }

        public string? AuthorizationUrl { get; set; }

        public string? Reference { get; set; }

        public string? ErrorMessage { get; set; }
    }
}
