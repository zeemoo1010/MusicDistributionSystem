using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MusicDistributionSystem.Application.Contracts.Services;
using MusicDistributionSystem.Domain.Constants;

namespace MusicDistributionSystem.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly IDashboardService _dashboardService;

        public DashboardController(IDashboardService dashboardService)
        {
            _dashboardService = dashboardService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userId, out var parsedUserId))
            {
                return Forbid();
            }

            var model = await _dashboardService.GetDashboardAsync(parsedUserId);
            if (model is null)
            {
                return NotFound();
            }

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteUpload(Guid id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userId, out var parsedUserId))
            {
                return Forbid();
            }

            var isPrivilegedUser = User.IsInRole(RoleNames.Admin) || User.IsInRole(RoleNames.SuperAdmin);
            var result = await _dashboardService.DeleteOwnUploadAsync(parsedUserId, id, isPrivilegedUser);
            TempData["StatusMessage"] = result.Succeeded
                ? "Upload deleted successfully."
                : result.ErrorMessage ?? "Unable to delete upload.";

            return RedirectToAction(nameof(Index));
        }
    }
}

