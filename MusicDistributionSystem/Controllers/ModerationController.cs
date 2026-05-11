using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MusicDistributionSystem.Application.DTOs.Moderation;
using MusicDistributionSystem.Application.Contracts.Services;

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
            var reviewerUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(reviewerUserId, out var parsedReviewerUserId))
            {
                return Forbid();
            }

            var result = await _moderationService.ApproveTrackAsync(id, parsedReviewerUserId);
            TempData["StatusMessage"] = result.Succeeded ? "Track approved successfully." : result.ErrorMessage;
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> Reject(RejectTrackRequestDto request)
        {
            if (!ModelState.IsValid)
            {
                TempData["StatusMessage"] = "A rejection reason is required.";
                return RedirectToAction(nameof(Index));
            }

            var reviewerUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(reviewerUserId, out var parsedReviewerUserId))
            {
                return Forbid();
            }

            var result = await _moderationService.RejectTrackAsync(request, parsedReviewerUserId);
            TempData["StatusMessage"] = result.Succeeded ? "Track rejected successfully." : result.ErrorMessage;
            return RedirectToAction(nameof(Index));
        }
    }
}

