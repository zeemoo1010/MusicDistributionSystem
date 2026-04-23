using MusicDistributionSystem.DTOs.Payment;
using MusicDistributionSystem.Logging.Interfaces;
using MusicDistributionSystem.Repositories.Interfaces;
using MusicDistributionSystem.Services.Interfaces;

namespace MusicDistributionSystem.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly IMembershipPlanRepository _membershipPlanRepository;
        private readonly IAppLogger _appLogger;

        public PaymentService(IMembershipPlanRepository membershipPlanRepository, IAppLogger appLogger)
        {
            _membershipPlanRepository = membershipPlanRepository;
            _appLogger = appLogger;
        }

        public async Task<PaymentCheckoutDto?> GetCheckoutAsync(Guid planId)
        {
            var plan = await _membershipPlanRepository.GetByIdAsync(planId);
            if (plan is null)
            {
                await _appLogger.LogWarningAsync("Payment", $"Checkout requested for unknown plan id '{planId}'.");
                return null;
            }

            await _appLogger.LogInformationAsync("Payment", $"Checkout opened for plan '{plan.Name}' ({plan.Id}).");

            return new PaymentCheckoutDto
            {
                PlanId = plan.Id,
                PlanName = plan.Name,
                TierName = plan.Tier.ToString(),
                MonthlyPrice = plan.MonthlyPrice,
                Description = plan.Description
            };
        }
    }
}
