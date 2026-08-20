using MusicDistributionSystem.Application.DTOs.Payment;

namespace MusicDistributionSystem.Application.Contracts.Services
{
    public interface IPaymentService
    {
        Task<IReadOnlyCollection<PaymentCheckoutDto>> GetPlansAsync();
        Task<PaymentCheckoutDto?> GetCheckoutAsync(Guid planId);
        Task<PaymentInitializationResultDto> InitializePaymentAsync(Guid planId, Guid userId, string email, string callbackUrl);
        Task<PaymentVerificationResultDto> VerifyPaymentAsync(string reference);
        Task HandleWebhookAsync(string eventName, string payload);
    }
}

