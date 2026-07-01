using MusicDistributionSystem.Application.Contracts.Services;
using MusicDistributionSystem.Application.DTOs.Payment;
using MusicDistributionSystem.Application.Contracts.Payments;
using MusicDistributionSystem.Domain.Contracts.Interface;
using MusicDistributionSystem.Domain.Contracts.Logging;

namespace MusicDistributionSystem.Application.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly IMembershipPlanRepository _membershipPlanRepository;
        private readonly IUserRepository _userRepository;
        private readonly IPaymentGateway _paymentGateway;
        private readonly IAppLogger _appLogger;

        public PaymentService(
            IMembershipPlanRepository membershipPlanRepository,
            IUserRepository userRepository,
            IPaymentGateway paymentGateway,
            IAppLogger appLogger)
        {
            _membershipPlanRepository = membershipPlanRepository;
            _userRepository = userRepository;
            _paymentGateway = paymentGateway;
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

        public async Task<PaymentInitializationResultDto> InitializePaymentAsync(Guid planId, Guid userId, string email, string callbackUrl)
        {
            var plan = await _membershipPlanRepository.GetByIdAsync(planId);
            if (plan is null)
            {
                await _appLogger.LogWarningAsync("Payment", $"Payment initialization requested for unknown plan id '{planId}'.");
                return new PaymentInitializationResultDto { ErrorMessage = "The selected plan could not be found." };
            }

            var user = await _userRepository.GetByIdAsync(userId);
            if (user is null || !user.IsActive || !user.IsEmailVerified)
            {
                await _appLogger.LogWarningAsync("Payment", $"Payment initialization blocked for invalid user '{userId}'.");
                return new PaymentInitializationResultDto { ErrorMessage = "Your account must be active before payment can be started." };
            }

            if (plan.MonthlyPrice <= 0)
            {
                user.MembershipTier = plan.Tier;
                await _userRepository.SaveChangesAsync();
                await _appLogger.LogInformationAsync("Payment", $"Free plan '{plan.Name}' activated for user '{user.Email}'.");

                return new PaymentInitializationResultDto
                {
                    Succeeded = true,
                    RequiresRedirect = false
                };
            }

            var reference = CreatePaymentReference(user.Id, plan.Id);
            var initialization = await _paymentGateway.InitializeAsync(new PaymentGatewayInitializeRequestDto
            {
                Email = email,
                Amount = plan.MonthlyPrice,
                Reference = reference,
                CallbackUrl = callbackUrl,
                UserId = user.Id,
                PlanId = plan.Id
            });

            if (!initialization.Succeeded || string.IsNullOrWhiteSpace(initialization.AuthorizationUrl))
            {
                await _appLogger.LogWarningAsync("Payment", $"Paystack initialization failed for plan '{plan.Id}' and user '{user.Id}'.");
                return new PaymentInitializationResultDto
                {
                    ErrorMessage = initialization.ErrorMessage ?? "Unable to start payment. Please try again."
                };
            }

            await _appLogger.LogInformationAsync("Payment", $"Paystack checkout initialized for plan '{plan.Name}' and user '{user.Email}'.");

            return new PaymentInitializationResultDto
            {
                Succeeded = true,
                RequiresRedirect = true,
                AuthorizationUrl = initialization.AuthorizationUrl
            };
        }

        public async Task<PaymentVerificationResultDto> VerifyPaymentAsync(string reference, Guid userId)
        {
            if (string.IsNullOrWhiteSpace(reference))
            {
                return new PaymentVerificationResultDto { ErrorMessage = "Payment reference is required." };
            }

            var verification = await _paymentGateway.VerifyAsync(reference.Trim());
            if (!verification.Succeeded || verification.Status != "success")
            {
                await _appLogger.LogWarningAsync("Payment", $"Paystack verification failed for reference '{reference}'.");
                return new PaymentVerificationResultDto
                {
                    ErrorMessage = verification.ErrorMessage ?? "Payment could not be verified."
                };
            }

            if (verification.UserId != userId || verification.PlanId is null)
            {
                await _appLogger.LogWarningAsync("Payment", $"Paystack verification metadata mismatch for reference '{reference}'.");
                return new PaymentVerificationResultDto { ErrorMessage = "Payment verification failed for this account." };
            }

            var plan = await _membershipPlanRepository.GetByIdAsync(verification.PlanId.Value);
            if (plan is null)
            {
                return new PaymentVerificationResultDto { ErrorMessage = "The paid plan could not be found." };
            }

            if (verification.Amount != plan.MonthlyPrice)
            {
                await _appLogger.LogWarningAsync("Payment", $"Paystack amount mismatch for reference '{reference}'.");
                return new PaymentVerificationResultDto { ErrorMessage = "Payment amount does not match the selected plan." };
            }

            var user = await _userRepository.GetByIdAsync(userId);
            if (user is null)
            {
                return new PaymentVerificationResultDto { ErrorMessage = "User account could not be found." };
            }

            user.MembershipTier = plan.Tier;
            await _userRepository.SaveChangesAsync();

            await _appLogger.LogInformationAsync("Payment", $"Payment verified and plan '{plan.Name}' activated for user '{user.Email}'.");

            return new PaymentVerificationResultDto
            {
                Succeeded = true,
                PlanName = plan.Name
            };
        }

        private static string CreatePaymentReference(Guid userId, Guid planId)
        {
            return $"MDS-{userId:N}-{planId:N}-{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}";
        }
    }
}

