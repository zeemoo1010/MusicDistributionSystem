using Microsoft.AspNetCore.Mvc;
using MusicDistributionSystem.Services.Interfaces;

namespace MusicDistributionSystem.Controllers
{
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
