using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MusicDistributionSystem.Application.DTOs.Admin;
using MusicDistributionSystem.Application.Contracts.Services;

namespace MusicDistributionSystem.Controllers
{
    [Authorize(Policy = "AdminOnly")]
    public class AdminController : Controller
    {
        private readonly IAdministrationService _administrationService;

        public AdminController(IAdministrationService administrationService)
        {
            _administrationService = administrationService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var model = await _administrationService.GetDashboardAsync();
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateUserRoles(UpdateUserRolesRequestDto request)
        {
            var result = await _administrationService.UpdateUserRolesAsync(request);
            TempData["StatusMessage"] = result.Succeeded
                ? "User roles updated successfully."
                : result.ErrorMessage ?? "Unable to update user roles.";

            return RedirectToAction(nameof(Index));
        }
    }
}

