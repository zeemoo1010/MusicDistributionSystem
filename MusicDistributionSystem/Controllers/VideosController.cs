using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MusicDistributionSystem.Application.Contracts.Services;
using MusicDistributionSystem.Application.DTOs.VideoDtos;
using System.Security.Claims;

namespace MusicDistributionSystem.Controllers
{
    public class VideosController : Controller
    {
        private readonly IVideoService _videoService;

        public VideosController(IVideoService videoService) => _videoService = videoService;

        [HttpGet("/Videos")]
        public async Task<IActionResult> Index(string? search, Guid? categoryId, int page = 1)
        {
            var model = await _videoService.GetVideoIndexAsync(search, categoryId, page, 12);
            return View(model);
        }

        [HttpGet("/Videos/Watch/{slug}")]
        public async Task<IActionResult> Watch(string slug)
        {
            var video = await _videoService.GetVideoDetailsBySlugAsync(slug);
            if (video is null) return NotFound();
            return View(video);
        }

        [HttpGet("/Videos/Upload")]
        [Authorize(Policy = "CanUploadContent")]
        public async Task<IActionResult> Upload()
        {
            var form = await _videoService.GetUploadFormAsync();
            return View(form);
        }

        [HttpPost("/Videos/Upload")]
        [Authorize(Policy = "CanUploadContent")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Upload(VideoUploadRequestDto request)
        {
            if (!ModelState.IsValid)
            {
                var form = await _videoService.GetUploadFormAsync();
                request.Categories = form.Categories;
                return View(request);
            }

            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var userName = User.Identity?.Name ?? "User";
            var userEmail = User.FindFirstValue(ClaimTypes.Email) ?? "user@soundsphere.com";

            var result = await _videoService.UploadAsync(request, userId, userName, userEmail);
            if (!result.Succeeded)
            {
                ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "Upload failed.");
                var form = await _videoService.GetUploadFormAsync();
                request.Categories = form.Categories;
                return View(request);
            }

            TempData["SuccessMessage"] = $"Video '{request.Title}' uploaded successfully!";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost("/Videos/Comment")]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddComment(Guid videoId, string content, string returnUrl)
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            await _videoService.AddCommentAsync(videoId, userId, content);
            return LocalRedirect(returnUrl ?? "/Videos");
        }

        [HttpPost("/Videos/Like")]
        [Authorize]
        public async Task<IActionResult> ToggleLike(Guid videoId)
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var result = await _videoService.ToggleLikeAsync(videoId, userId);
            return Json(new { success = result.Succeeded });
        }
    }
}
