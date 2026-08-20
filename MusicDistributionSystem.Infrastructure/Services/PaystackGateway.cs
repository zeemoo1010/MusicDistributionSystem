using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Options;
using MusicDistributionSystem.Application.Contracts.Payments;
using MusicDistributionSystem.Application.DTOs.Payment;
using MusicDistributionSystem.Infrastructure.Configuration;
using MusicDistributionSystem.Domain.Contracts.Logging;

namespace MusicDistributionSystem.Infrastructure.Services
{
    public class PaystackGateway : IPaymentGateway
    {
        private readonly HttpClient _httpClient;
        private readonly IAppLogger _logger;

        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        private readonly string _secretKey;

        public PaystackGateway(IHttpClientFactory httpClientFactory, IOptions<PaystackSettings> settings, IAppLogger logger)
        {
            _httpClient = httpClientFactory.CreateClient("Paystack");
            _logger = logger;
            _secretKey = settings.Value.SecretKey ?? string.Empty;

            if (!string.IsNullOrWhiteSpace(_secretKey))
            {
                _httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", _secretKey);
            }
            _httpClient.BaseAddress = new Uri("https://api.paystack.co");
        }

        public async Task<PaymentGatewayInitializeResultDto> InitializeAsync(
            PaymentGatewayInitializeRequestDto request, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(_secretKey))
            {
                await _logger.LogWarningAsync("Paystack", "Gateway init attempted without Paystack SecretKey configured.");
                return new PaymentGatewayInitializeResultDto { ErrorMessage = "Paystack gateway key is not configured." };
            }

            var amountInKobo = (int)(request.Amount * 100);

            var body = new
            {
                email = request.Email,
                amount = amountInKobo,
                callback_url = request.CallbackUrl,
                reference = request.Reference,
                metadata = new { user_id = request.UserId.ToString(), plan_id = request.PlanId.ToString() }
            };

            try
            {
                var json = JsonSerializer.Serialize(body, JsonOptions);
                using var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync("/transaction/initialize", content, cancellationToken);
                var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);
                var result = JsonSerializer.Deserialize<PaystackInitResponse>(responseBody, JsonOptions);

                if (result?.Status is true && result.Data?.AuthorizationUrl is not null)
                {
                    return new PaymentGatewayInitializeResultDto
                    {
                        Succeeded = true,
                        AuthorizationUrl = result.Data.AuthorizationUrl,
                        Reference = result.Data.Reference
                    };
                }

                return new PaymentGatewayInitializeResultDto { ErrorMessage = result?.Message ?? "Initialization failed." };
            }
            catch (Exception ex)
            {
                await _logger.LogErrorAsync("Paystack", $"Gateway init failed: {ex.Message}", ex);
                return new PaymentGatewayInitializeResultDto { ErrorMessage = "Payment service error." };
            }
        }

        public async Task<PaymentGatewayVerificationResultDto> VerifyAsync(string reference, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(_secretKey))
            {
                await _logger.LogWarningAsync("Paystack", $"Gateway verify attempted for reference '{reference}' without Paystack SecretKey configured.");
                return new PaymentGatewayVerificationResultDto { ErrorMessage = "Paystack gateway key is not configured." };
            }
            try
            {
                var response = await _httpClient.GetAsync($"/transaction/verify/{Uri.EscapeDataString(reference)}", cancellationToken);
                var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);
                var result = JsonSerializer.Deserialize<PaystackVerifyResponse>(responseBody, JsonOptions);

                if (result?.Status is true && result.Data is not null)
                {
                    return new PaymentGatewayVerificationResultDto
                    {
                        Succeeded = result.Data.Status == "success",
                        Status = result.Data.Status,
                        Reference = result.Data.Reference,
                        Amount = result.Data.Amount / 100m,
                        Currency = result.Data.Currency ?? "NGN",
                        UserId = result.Data.Metadata?.UserId,
                        PlanId = result.Data.Metadata?.PlanId,
                        RawResponse = responseBody
                    };
                }

                return new PaymentGatewayVerificationResultDto { ErrorMessage = result?.Message ?? "Verification failed." };
            }
            catch (Exception ex)
            {
                await _logger.LogErrorAsync("Paystack", $"Gateway verify failed for '{reference}': {ex.Message}", ex);
                return new PaymentGatewayVerificationResultDto { ErrorMessage = "Verification service error." };
            }
        }

        private class PaystackInitResponse { public bool Status { get; set; } public string? Message { get; set; } public PaystackInitData? Data { get; set; } }
        private class PaystackInitData { public string? AuthorizationUrl { get; set; } public string? Reference { get; set; } }
        private class PaystackVerifyResponse { public bool Status { get; set; } public string? Message { get; set; } public PaystackVerifyData? Data { get; set; } }
        private class PaystackVerifyData { public string? Status { get; set; } public string? Reference { get; set; } public decimal Amount { get; set; } public string? Currency { get; set; } public string? GatewayResponse { get; set; } public PaystackVerifyMetadata? Metadata { get; set; } }
        private class PaystackVerifyMetadata { public Guid? UserId { get; set; } public Guid? PlanId { get; set; } }
    }
}