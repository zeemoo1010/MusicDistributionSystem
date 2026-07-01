using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Options;
using MusicDistributionSystem.Application.Contracts.Services;
using MusicDistributionSystem.Application.DTOs.Payment;
using MusicDistributionSystem.Domain.Contracts.Interface;
using MusicDistributionSystem.Domain.Contracts.Logging;
using MusicDistributionSystem.Infrastructure.Configuration;

namespace MusicDistributionSystem.Infrastructure.Services
{
    public class PaystackService : IPaymentService
    {
        private readonly HttpClient _httpClient;
        private readonly IMembershipPlanRepository _membershipPlanRepository;
        private readonly IAppLogger _logger;

        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        public PaystackService(
            IHttpClientFactory httpClientFactory,
            IOptions<PaystackSettings> settings,
            IMembershipPlanRepository membershipPlanRepository,
            IAppLogger logger)
        {
            _httpClient = httpClientFactory.CreateClient("Paystack");
            _membershipPlanRepository = membershipPlanRepository;
            _logger = logger;

            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", settings.Value.SecretKey);
            _httpClient.BaseAddress = new Uri("https://api.paystack.co");
        }

        public async Task<PaymentCheckoutDto?> GetCheckoutAsync(Guid planId)
        {
            var plan = await _membershipPlanRepository.GetByIdAsync(planId);
            if (plan is null)
            {
                await _logger.LogWarningAsync("Payment", $"Checkout requested for unknown plan id '{planId}'.");
                return null;
            }

            await _logger.LogInformationAsync("Payment", $"Checkout opened for plan '{plan.Name}' ({plan.Id}).");

            return new PaymentCheckoutDto
            {
                PlanId = plan.Id,
                PlanName = plan.Name,
                TierName = plan.Tier.ToString(),
                MonthlyPrice = plan.MonthlyPrice,
                Description = plan.Description
            };
        }

        public async Task<PaymentInitializationResultDto> InitializePaymentAsync(
            Guid planId, Guid userId, string email, string callbackUrl)
        {
            var plan = await _membershipPlanRepository.GetByIdAsync(planId);
            if (plan is null)
            {
                return new PaymentInitializationResultDto
                {
                    Succeeded = false,
                    ErrorMessage = "Payment plan not found."
                };
            }

            var amountInKobo = (int)(plan.MonthlyPrice * 100);
            var reference = $"SUB_{userId:N}_{planId:N}_{DateTime.UtcNow:yyyyMMddHHmmss}";

            var requestBody = new
            {
                email,
                amount = amountInKobo,
                callback_url = callbackUrl,
                reference
            };

            try
            {
                var json = JsonSerializer.Serialize(requestBody, JsonOptions);
                using var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync("/transaction/initialize", content);
                var responseBody = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<PaystackInitializeResponse>(responseBody, JsonOptions);

                if (result?.Status is true && result.Data?.AuthorizationUrl is not null)
                {
                    return new PaymentInitializationResultDto
                    {
                        Succeeded = true,
                        RequiresRedirect = true,
                        AuthorizationUrl = result.Data.AuthorizationUrl
                    };
                }

                return new PaymentInitializationResultDto
                {
                    Succeeded = false,
                    ErrorMessage = result?.Message ?? "Payment initialization failed."
                };
            }
            catch (Exception ex)
            {
                await _logger.LogErrorAsync("Paystack", $"Payment initialization failed for {email}: {ex.Message}", ex);
                return new PaymentInitializationResultDto
                {
                    Succeeded = false,
                    ErrorMessage = "An error occurred while processing payment."
                };
            }
        }

        public async Task<PaymentVerificationResultDto> VerifyPaymentAsync(string reference, Guid userId)
        {
            try
            {
                var response = await _httpClient.GetAsync($"/transaction/verify/{Uri.EscapeDataString(reference)}");
                var responseBody = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<PaystackVerifyResponse>(responseBody, JsonOptions);

                if (result?.Status is true && result.Data?.Status == "success")
                {
                    return new PaymentVerificationResultDto
                    {
                        Succeeded = true
                    };
                }

                var gatewayResponse = result?.Data?.GatewayResponse;
                return new PaymentVerificationResultDto
                {
                    Succeeded = false,
                    ErrorMessage = gatewayResponse ?? "Payment verification failed."
                };
            }
            catch (Exception ex)
            {
                await _logger.LogErrorAsync("Paystack", $"Verification failed for reference '{reference}': {ex.Message}", ex);
                return new PaymentVerificationResultDto
                {
                    Succeeded = false,
                    ErrorMessage = "Verification failed due to an error."
                };
            }
        }

        private class PaystackInitializeResponse
        {
            public bool Status { get; set; }
            public string? Message { get; set; }
            public PaystackInitializeData? Data { get; set; }
        }

        private class PaystackInitializeData
        {
            [JsonPropertyName("authorization_url")]
            public string? AuthorizationUrl { get; set; }
            public string? Reference { get; set; }
            public string? AccessCode { get; set; }
        }

        private class PaystackVerifyResponse
        {
            public bool Status { get; set; }
            public string? Message { get; set; }
            public PaystackVerifyData? Data { get; set; }
        }

        private class PaystackVerifyData
        {
            public string? Status { get; set; }
            [JsonPropertyName("gateway_response")]
            public string? GatewayResponse { get; set; }
        }
    }
}