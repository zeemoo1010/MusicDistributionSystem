using MusicDistributionSystem.Application.DTOs.Payment;

namespace MusicDistributionSystem.Application.Contracts.Payments
{
    public interface IPaymentGateway
    {
        Task<PaymentGatewayInitializeResultDto> InitializeAsync(
            PaymentGatewayInitializeRequestDto request,
            CancellationToken cancellationToken = default);

        Task<PaymentGatewayVerificationResultDto> VerifyAsync(
            string reference,
            CancellationToken cancellationToken = default);
    }
}
