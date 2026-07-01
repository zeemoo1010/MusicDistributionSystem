using MusicDistributionSystem.Application.DTOs.Payment;

namespace MusicDistributionSystem.Application.Contracts.Services
{
    public interface IPaymentService
    {
        Task<PaymentCheckoutDto?> GetCheckoutAsync(Guid planId);
        Task<PaymentInitializationResultDto> InitializePaymentAsync(Guid planId, Guid userId, string email, string callbackUrl);
        Task<PaymentVerificationResultDto> VerifyPaymentAsync(string reference, Guid userId);
    }
}

