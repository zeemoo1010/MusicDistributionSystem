using MusicDistributionSystem.DTOs.Payment;

namespace MusicDistributionSystem.Services.Interfaces
{
    public interface IPaymentService
    {
        Task<PaymentCheckoutDto?> GetCheckoutAsync(Guid planId);
    }
}
