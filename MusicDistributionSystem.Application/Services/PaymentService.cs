using System.Text.Json;
using MusicDistributionSystem.Application.Contracts.Services;
using MusicDistributionSystem.Application.DTOs.Payment;
using MusicDistributionSystem.Application.Contracts.Payments;
using MusicDistributionSystem.Domain.Contracts.Interface;
using MusicDistributionSystem.Domain.Contracts.Logging;
using MusicDistributionSystem.Domain.Entities;
using MusicDistributionSystem.Domain.Enums;

namespace MusicDistributionSystem.Application.Services
{
    public class PaymentService : IPaymentService
    {
        private const int SubscriptionMonths = 1;

        private readonly IMembershipPlanRepository _membershipPlanRepository;
        private readonly IUserRepository _userRepository;
        private readonly IPaymentTransactionRepository _paymentTransactionRepository;
        private readonly IUserSubscriptionRepository _userSubscriptionRepository;
        private readonly IPaymentGateway _paymentGateway;
        private readonly IAccountNotificationService _accountNotificationService;
        private readonly IAppLogger _appLogger;

        public PaymentService(
            IMembershipPlanRepository membershipPlanRepository,
            IUserRepository userRepository,
            IPaymentTransactionRepository paymentTransactionRepository,
            IUserSubscriptionRepository userSubscriptionRepository,
            IPaymentGateway paymentGateway,
            IAccountNotificationService accountNotificationService,
            IAppLogger appLogger)
        {
            _membershipPlanRepository = membershipPlanRepository;
            _userRepository = userRepository;
            _paymentTransactionRepository = paymentTransactionRepository;
            _userSubscriptionRepository = userSubscriptionRepository;
            _paymentGateway = paymentGateway;
            _accountNotificationService = accountNotificationService;
            _appLogger = appLogger;
        }

        public async Task<IReadOnlyCollection<PaymentCheckoutDto>> GetPlansAsync()
        {
            var plans = await _membershipPlanRepository.GetAllAsync();
            return plans.Select(p => new PaymentCheckoutDto
            {
                PlanId = p.Id,
                PlanName = p.Name,
                TierName = p.Tier.ToString(),
                MonthlyPrice = p.MonthlyPrice,
                Description = p.Description
            }).ToArray();
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
                await ActivatePlanAsync(user, plan, reference: null, rawResponse: null, isFreePlan: true);
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

            // Persist a Pending transaction so the reference is known before the redirect
            await _paymentTransactionRepository.AddAsync(new PaymentTransaction
            {
                UserId = user.Id,
                PlanId = plan.Id,
                Amount = plan.MonthlyPrice,
                Currency = "NGN",
                Status = PaymentStatus.Pending,
                Reference = reference
            });
            await _paymentTransactionRepository.SaveChangesAsync();

            await _appLogger.LogInformationAsync("Payment", $"Paystack checkout initialized for plan '{plan.Name}' and user '{user.Email}'.");

            return new PaymentInitializationResultDto
            {
                Succeeded = true,
                RequiresRedirect = true,
                AuthorizationUrl = initialization.AuthorizationUrl
            };
        }

        public async Task<PaymentVerificationResultDto> VerifyPaymentAsync(string reference)
        {
            if (string.IsNullOrWhiteSpace(reference))
            {
                return new PaymentVerificationResultDto { ErrorMessage = "Payment reference is required." };
            }

            var trimmedRef = reference.Trim();

            // Idempotency check: if reference was already verified successfully in DB
            var existingTx = await _paymentTransactionRepository.GetByReferenceAsync(trimmedRef);
            if (existingTx is not null && existingTx.Status == PaymentStatus.Success)
            {
                var existingPlan = existingTx.PlanId.HasValue 
                    ? await _membershipPlanRepository.GetByIdAsync(existingTx.PlanId.Value) 
                    : null;
                await _appLogger.LogInformationAsync("Payment", $"Payment reference '{trimmedRef}' already processed successfully.");
                return new PaymentVerificationResultDto
                {
                    Succeeded = true,
                    PlanName = existingPlan?.Name ?? "Subscription Plan"
                };
            }

            var verification = await _paymentGateway.VerifyAsync(trimmedRef);
            if (!verification.Succeeded || verification.Status != "success")
            {
                await _appLogger.LogWarningAsync("Payment", $"Paystack verification failed for reference '{trimmedRef}'.");
                return new PaymentVerificationResultDto
                {
                    ErrorMessage = verification.ErrorMessage ?? "Payment could not be verified."
                };
            }

            if (verification.UserId is null || verification.PlanId is null)
            {
                await _appLogger.LogWarningAsync("Payment", $"Paystack verification missing metadata for reference '{trimmedRef}'.");
                return new PaymentVerificationResultDto { ErrorMessage = "Payment verification failed for this account." };
            }

            var user = await _userRepository.GetByIdAsync(verification.UserId.Value);
            if (user is null)
            {
                return new PaymentVerificationResultDto { ErrorMessage = "User account could not be found." };
            }

            var plan = await _membershipPlanRepository.GetByIdAsync(verification.PlanId.Value);
            if (plan is null)
            {
                return new PaymentVerificationResultDto { ErrorMessage = "The paid plan could not be found." };
            }

            if (verification.Amount != plan.MonthlyPrice)
            {
                await _appLogger.LogWarningAsync("Payment", $"Paystack amount mismatch for reference '{trimmedRef}'.");
                return new PaymentVerificationResultDto { ErrorMessage = "Payment amount does not match the selected plan." };
            }

            await ActivatePlanAsync(user, plan, verification.Reference ?? trimmedRef, verification.RawResponse, isFreePlan: false);

            await _appLogger.LogInformationAsync("Payment", $"Payment verified and plan '{plan.Name}' activated for user '{user.Email}'.");

            try
            {
                await _accountNotificationService.SendPaymentReceiptAsync(
                    user.Email,
                    plan.Name,
                    plan.MonthlyPrice,
                    verification.Reference ?? trimmedRef);
            }
            catch (Exception ex)
            {
                await _appLogger.LogWarningAsync("Payment", $"Failed to send payment receipt email to '{user.Email}': {ex.Message}");
            }

            return new PaymentVerificationResultDto
            {
                Succeeded = true,
                PlanName = plan.Name
            };
        }

        public async Task HandleWebhookAsync(string eventName, string payload)
        {
            if (eventName == "charge.success")
            {
                var reference = ExtractReferenceFromWebhook(payload);
                if (reference is null)
                {
                    await _appLogger.LogWarningAsync("Payment", "Webhook charge.success received without a reference.");
                    return;
                }

                await VerifyPaymentAsync(reference);
                return;
            }

            await _appLogger.LogInformationAsync("Payment", $"Unhandled Paystack webhook event '{eventName}'.");
        }

        private async Task ActivatePlanAsync(User user, MembershipPlan plan, string? reference, string? rawResponse, bool isFreePlan)
        {
            user.MembershipTier = plan.Tier;

            if (isFreePlan)
            {
                var existingFreeSubscription = await _userSubscriptionRepository.GetActiveByUserAsync(user.Id);
                if (existingFreeSubscription is null)
                {
                    await _userSubscriptionRepository.AddAsync(new UserSubscription
                    {
                        UserId = user.Id,
                        PlanId = plan.Id,
                        StartDateUtc = DateTime.UtcNow,
                        EndDateUtc = DateTime.UtcNow.AddMonths(SubscriptionMonths),
                        IsActive = true,
                        AutoRenew = false
                    });
                }
            }
            else if (reference is not null)
            {
                var existingTransaction = await _paymentTransactionRepository.GetByReferenceAsync(reference);
                if (existingTransaction is null)
                {
                    await _paymentTransactionRepository.AddAsync(new PaymentTransaction
                    {
                        UserId = user.Id,
                        PlanId = plan.Id,
                        Amount = plan.MonthlyPrice,
                        Currency = "NGN",
                        Status = PaymentStatus.Success,
                        Reference = reference,
                        PaystackResponse = rawResponse,
                        PaidAtUtc = DateTime.UtcNow
                    });
                }
                else
                {
                    existingTransaction.Status = PaymentStatus.Success;
                    existingTransaction.PaystackResponse = rawResponse;
                    existingTransaction.PaidAtUtc = DateTime.UtcNow;
                }

                var activeSubscription = await _userSubscriptionRepository.GetActiveByUserAsync(user.Id);
                if (activeSubscription is null || activeSubscription.PlanId != plan.Id)
                {
                    if (activeSubscription is not null)
                    {
                        activeSubscription.IsActive = false;
                    }

                    await _userSubscriptionRepository.AddAsync(new UserSubscription
                    {
                        UserId = user.Id,
                        PlanId = plan.Id,
                        StartDateUtc = DateTime.UtcNow,
                        EndDateUtc = DateTime.UtcNow.AddMonths(SubscriptionMonths),
                        IsActive = true,
                        AutoRenew = false,
                        PaystackSubscriptionCode = reference
                    });
                }
            }

            await _userRepository.SaveChangesAsync();
            await _paymentTransactionRepository.SaveChangesAsync();
            await _userSubscriptionRepository.SaveChangesAsync();
        }

        private static string? ExtractReferenceFromWebhook(string payload)
        {
            try
            {
                using var document = JsonDocument.Parse(payload);
                if (document.RootElement.TryGetProperty("data", out var data) &&
                    data.TryGetProperty("reference", out var reference))
                {
                    return reference.GetString();
                }
            }
            catch (JsonException)
            {
                // Invalid JSON — the webhook body is not parseable.
            }

            return null;
        }

        private static string CreatePaymentReference(Guid userId, Guid planId)
        {
            return $"MDS-{userId:N}-{planId:N}-{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}";
        }
    }
}
