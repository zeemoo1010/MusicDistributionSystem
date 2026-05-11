using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MusicDistributionSystem.Application.DTOs.Admin;
using MusicDistributionSystem.Application.Contracts.Services;
using MusicDistributionSystem.Domain.Constants;

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
            var result = await _administrationService.UpdateUserRolesAsync(request, User.IsInRole(RoleNames.SuperAdmin));
            TempData["StatusMessage"] = result.Succeeded
                ? "User roles updated successfully."
                : result.ErrorMessage ?? "Unable to update user roles.";

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> CreateCategory(CreateCategoryRequestDto request)
        {
            if (!ModelState.IsValid)
            {
                TempData["StatusMessage"] = "Please provide a valid category name.";
                return RedirectToAction(nameof(Index));
            }

            var result = await _administrationService.CreateCategoryAsync(request);
            TempData["StatusMessage"] = result.Succeeded
                ? "Category created successfully."
                : result.ErrorMessage ?? "Unable to create the category.";

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> UpdateCategory(UpdateCategoryRequestDto request)
        {
            if (!ModelState.IsValid)
            {
                TempData["StatusMessage"] = "Please provide a valid category update.";
                return RedirectToAction(nameof(Index));
            }

            var result = await _administrationService.UpdateCategoryAsync(request);
            TempData["StatusMessage"] = result.Succeeded
                ? "Category updated successfully."
                : result.ErrorMessage ?? "Unable to update the category.";

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> DeleteCategory(Guid id)
        {
            var result = await _administrationService.DeleteCategoryAsync(id);
            TempData["StatusMessage"] = result.Succeeded
                ? "Category deleted successfully."
                : result.ErrorMessage ?? "Unable to delete the category.";

            return RedirectToAction(nameof(Index));
        }
    }
}

