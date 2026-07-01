using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MusicDistributionSystem.Application.Contracts.Services;

namespace MusicDistributionSystem.Controllers
{
    [Authorize]
    public class PaymentController : Controller
    {
        private readonly IPaymentService _paymentService;

        public PaymentController(IPaymentService paymentService)
        {
            _paymentService = paymentService;
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

            var userId = User.FindFirstValue(System.Security.Claims.ClaimTypes.NameIdentifier);
            var result = await _paymentService.VerifyPaymentAsync(
                reference, Guid.Parse(userId ?? Guid.Empty.ToString()));

            if (result.Succeeded)
            {
                TempData["StatusMessage"] = "Payment verified successfully! Your subscription is now active.";
            }
            else
            {
                TempData["StatusMessage"] = result.ErrorMessage ?? "Payment verification failed.";
            }

            return RedirectToAction("Index", "Home");
        }
    }
}