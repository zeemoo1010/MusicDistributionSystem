using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MusicDistributionSystem.Application.Contracts.Services;
using MusicDistributionSystem.Application.DTOs.ImageDtos;
using System.Security.Claims;

namespace MusicDistributionSystem.Controllers
{
    public class ImagesController : Controller
    {
        private readonly IImageService _imageService;

        public ImagesController(IImageService imageService) => _imageService = imageService;

        [HttpGet("/Images")]
        public async Task<IActionResult> Index(string? search, Guid? categoryId, int page = 1)
        {
            var model = await _imageService.GetImageIndexAsync(search, categoryId, page, 16);
            return View(model);
        }

        [HttpGet("/Images/Details/{slug}")]
        public async Task<IActionResult> Details(string slug)
        {
            var image = await _imageService.GetImageDetailsBySlugAsync(slug);
            if (image is null) return NotFound();
            return View(image);
        }

        [HttpGet("/Images/Upload")]
        [Authorize(Policy = "CanUploadContent")]
        public async Task<IActionResult> Upload()
        {
            var form = await _imageService.GetUploadFormAsync();
            return View(form);
        }

        [HttpPost("/Images/Upload")]
        [Authorize(Policy = "CanUploadContent")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Upload(ImageUploadRequestDto request)
        {
            if (!ModelState.IsValid)
            {
                var form = await _imageService.GetUploadFormAsync();
                request.Categories = form.Categories;
                return View(request);
            }

            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var userName = User.Identity?.Name ?? "User";
            var userEmail = User.FindFirstValue(ClaimTypes.Email) ?? "user@soundsphere.com";

            var result = await _imageService.UploadAsync(request, userId, userName, userEmail);
            if (!result.Succeeded)
            {
                ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "Upload failed.");
                var form = await _imageService.GetUploadFormAsync();
                request.Categories = form.Categories;
                return View(request);
            }

            TempData["SuccessMessage"] = $"Image '{request.Title}' uploaded successfully!";
            return RedirectToAction(nameof(Index));
        }
    }
}
