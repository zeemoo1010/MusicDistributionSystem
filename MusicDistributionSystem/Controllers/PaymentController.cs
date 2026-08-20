using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MusicDistributionSystem.Application.Contracts.Services;

namespace MusicDistributionSystem.Controllers
{
    [Authorize]
    public class PaymentController : Controller
    {
        private readonly IPaymentService _paymentService;
        private readonly IConfiguration _configuration;

        public PaymentController(IPaymentService paymentService, IConfiguration configuration)
        {
            _paymentService = paymentService;
            _configuration = configuration;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> Plans()
        {
            var plans = await _paymentService.GetPlansAsync();
            return View(plans);
        }

        [HttpGet]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> Checkout(Guid planId)
        {
            var model = await _paymentService.GetCheckoutAsync(planId);

            if (model is null)
            {
                return NotFound();
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Initialize(Guid planId)
        {
            var userId = User.FindFirstValue(System.Security.Claims.ClaimTypes.NameIdentifier);
            var email = User.FindFirstValue(System.Security.Claims.ClaimTypes.Email);

            if (string.IsNullOrWhiteSpace(email))
            {
                return BadRequest("Email address is required for payment.");
            }

            var callbackUrl = Url.Action("Callback", "Payment", null, Request.Scheme) ??
                $"{Request.Scheme}://{Request.Host}/Payment/Callback";

            var result = await _paymentService.InitializePaymentAsync(
                planId, Guid.Parse(userId!), email, callbackUrl);

            if (!result.Succeeded)
            {
                TempData["StatusMessage"] = result.ErrorMessage ?? "Payment initialization failed.";
                return RedirectToAction("Index", "Home");
            }

            return Redirect(result.AuthorizationUrl!);
        }

        [HttpGet]
        [AllowAnonymous]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> Callback(string reference)
        {
            if (string.IsNullOrWhiteSpace(reference))
            {
                return BadRequest("Payment reference is required.");
            }

            var result = await _paymentService.VerifyPaymentAsync(reference);

            TempData["StatusMessage"] = result.Succeeded
                ? "Payment verified successfully! Your subscription is now active."
                : result.ErrorMessage ?? "Payment verification failed.";

            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        [AllowAnonymous]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> Webhook()
        {
            var secretKey = _configuration["Paystack:SecretKey"];
            if (string.IsNullOrWhiteSpace(secretKey))
            {
                return BadRequest("Paystack webhook is not configured.");
            }

            if (!Request.Headers.TryGetValue("x-paystack-signature", out var signatureHeader))
            {
                return BadRequest("Missing Paystack signature.");
            }

            string body;
            using (var reader = new StreamReader(Request.Body, Encoding.UTF8))
            {
                body = await reader.ReadToEndAsync();
            }

            var expectedSignature = ComputeHmacSha512(secretKey, body);
            var providedSignature = signatureHeader.ToString();

            if (!CryptographicOperations.FixedTimeEquals(
                    Encoding.UTF8.GetBytes(expectedSignature),
                    Encoding.UTF8.GetBytes(providedSignature)))
            {
                return Unauthorized("Invalid Paystack signature.");
            }

            string eventName;
            try
            {
                using var document = System.Text.Json.JsonDocument.Parse(body);
                eventName = document.RootElement.GetProperty("event").GetString() ?? string.Empty;
            }
            catch (Exception)
            {
                return BadRequest("Invalid webhook payload.");
            }

            await _paymentService.HandleWebhookAsync(eventName, body);
            return Ok();
        }

        private static string ComputeHmacSha512(string key, string message)
        {
            using var hmac = new HMACSHA512(Encoding.UTF8.GetBytes(key));
            var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(message));
            return Convert.ToHexString(hash).ToLowerInvariant();
        }
    }
}
