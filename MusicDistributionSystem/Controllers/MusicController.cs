using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MusicDistributionSystem.DTOs.Music;
using MusicDistributionSystem.Services.Interfaces;

namespace MusicDistributionSystem.Controllers
{
    public class MusicController : Controller
    {
        private readonly IMusicService _musicService;

        public MusicController(IMusicService musicService)
        {
            _musicService = musicService;
        }

        public async Task<IActionResult> Index(string? searchTerm, Guid? categoryId)
        {
            var model = await _musicService.GetMusicIndexAsync(searchTerm, categoryId);
            return View(model);
        }

        public async Task<IActionResult> Details(Guid id)
        {
            var track = await _musicService.GetMusicDetailsAsync(id);
            if (track is null)
            {
                return NotFound();
            }

            return View(track);
        }

        [Authorize(Policy = "CanUploadContent")]
        [HttpGet]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> Upload()
        {
            var model = await _musicService.GetUploadFormAsync();
            return View(model);
        }

        [Authorize(Policy = "CanUploadContent")]
        [HttpPost]
        public async Task<IActionResult> Upload(MusicUploadRequestDto request)
        {
            if (!ModelState.IsValid)
            {
                var hydratedRequest = await _musicService.GetUploadFormAsync();
                CopyRequestValues(request, hydratedRequest);
                return View(hydratedRequest);
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var email = User.FindFirstValue(ClaimTypes.Email);
            var username = User.Identity?.Name;

            if (!Guid.TryParse(userId, out var parsedUserId) || string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(username))
            {
                return Forbid();
            }

            var result = await _musicService.UploadAsync(request, parsedUserId, username, email);
            if (!result.Succeeded)
            {
                ModelState.AddModelError(nameof(request.MusicFile), result.ErrorMessage ?? "Upload failed.");
                var hydratedRequest = await _musicService.GetUploadFormAsync();
                CopyRequestValues(request, hydratedRequest);
                return View(hydratedRequest);
            }

            TempData["StatusMessage"] = "Upload received successfully. It is now awaiting admin approval.";
            return RedirectToAction(nameof(Index));
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Download(Guid id)
        {
            var result = await _musicService.PrepareDownloadAsync(id, HttpContext.Connection.RemoteIpAddress?.ToString());

            if (!result.Found)
            {
                return NotFound();
            }

            if (!result.Allowed || !result.FileExists)
            {
                TempData["StatusMessage"] = result.ErrorMessage;
                return RedirectToAction(nameof(Details), new { id });
            }

            return PhysicalFile(result.FilePath, result.ContentType, result.OriginalFileName);
        }

        private static void CopyRequestValues(MusicUploadRequestDto source, MusicUploadRequestDto destination)
        {
            destination.Title = source.Title;
            destination.Artist = source.Artist;
            destination.Description = source.Description;
            destination.CategoryId = source.CategoryId;
            destination.AccessLevel = source.AccessLevel;
            destination.MusicFile = source.MusicFile;
        }
    }
}
