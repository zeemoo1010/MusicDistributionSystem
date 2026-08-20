using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MusicDistributionSystem.Application.DTOs.Music;
using MusicDistributionSystem.Application.Contracts.Services;

namespace MusicDistributionSystem.Controllers
{
    public class MusicController : Controller
    {
        private readonly IMusicService _musicService;

        public MusicController(IMusicService musicService)
        {
            _musicService = musicService;
        }

        public async Task<IActionResult> Index(string? searchTerm, Guid? categoryId, int page = 1)
        {
            var model = await _musicService.GetMusicIndexAsync(searchTerm, categoryId, page, 12);
            return View(model);
        }

        [HttpGet("/Music/Details/{id:guid}")]
        public async Task<IActionResult> Details(Guid id)
        {
            var track = await _musicService.GetMusicDetailsAsync(id);
            if (track is null) return NotFound();
            return View(track);
        }

        [HttpGet("/Music/Track/{slug}")]
        public async Task<IActionResult> Track(string slug)
        {
            var track = await _musicService.GetMusicDetailsBySlugAsync(slug);
            if (track is null) return NotFound();
            return View("Details", track);
        }

        [HttpPost("/Music/Comment")]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddComment(Guid trackId, string content, string returnUrl)
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            await _musicService.AddCommentAsync(trackId, userId, content);
            return LocalRedirect(returnUrl ?? $"/Music/Details/{trackId}");
        }

        [HttpPost("/Music/Like")]
        [Authorize]
        public async Task<IActionResult> ToggleLike(Guid trackId)
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var result = await _musicService.ToggleLikeAsync(trackId, userId);
            return Json(new { success = result.Succeeded });
        }

        [Authorize(Policy = "CanUploadContent")]
        [HttpGet]
        public async Task<IActionResult> Upload()
        {
            var model = await _musicService.GetUploadFormAsync();
            return View(model);
        }

        [Authorize(Policy = "CanUploadContent")]
        [HttpPost]
        [ValidateAntiForgeryToken]
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

            TempData["SuccessMessage"] = $"Track '{request.Title}' uploaded successfully!";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost("/Music/Download/{id:guid}")]
        [HttpGet("/Music/Download/{id:guid}")]
        public async Task<IActionResult> Download(Guid id)
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            Guid? userId = Guid.TryParse(userIdClaim, out var parsedUserId) ? parsedUserId : null;

            var result = await _musicService.PrepareDownloadAsync(id, userId, HttpContext.Connection.RemoteIpAddress?.ToString());

            if (!result.Found)
            {
                return NotFound("Track not found.");
            }

            if (!result.Allowed)
            {
                TempData["ErrorMessage"] = result.ErrorMessage;
                return RedirectToAction("Plans", "Payment");
            }

            if (!result.FileExists)
            {
                TempData["ErrorMessage"] = "Audio file is missing on the server.";
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
            destination.CoverImage = source.CoverImage;
        }
    }
}


