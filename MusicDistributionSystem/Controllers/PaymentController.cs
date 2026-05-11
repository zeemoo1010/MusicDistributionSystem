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
    }
}

