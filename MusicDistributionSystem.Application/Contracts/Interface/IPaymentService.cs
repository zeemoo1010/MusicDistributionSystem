using MusicDistributionSystem.Application.DTOs.Payment;

namespace MusicDistributionSystem.Application.Contracts.Services
{
    public interface IPaymentService
    {
        Task<PaymentCheckoutDto?> GetCheckoutAsync(Guid planId);
    }
}

