using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MusicDistributionSystem.Services.Interfaces;

namespace MusicDistributionSystem.Controllers
{
    [Authorize(Policy = "CanModerateContent")]
    public class ModerationController : Controller
    {
        private readonly IModerationService _moderationService;

        public ModerationController(IModerationService moderationService)
        {
            _moderationService = moderationService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var model = await _moderationService.GetDashboardAsync();
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Approve(Guid id)
        {
            var result = await _moderationService.ApproveTrackAsync(id);
            TempData["StatusMessage"] = result.Succeeded ? "Track approved successfully." : result.ErrorMessage;
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> Reject(Guid id)
        {
            var result = await _moderationService.RejectTrackAsync(id);
            TempData["StatusMessage"] = result.Succeeded ? "Track rejected successfully." : result.ErrorMessage;
            return RedirectToAction(nameof(Index));
        }
    }
}
