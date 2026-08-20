using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MusicDistributionSystem.Application.Contracts.Services;
using MusicDistributionSystem.Application.DTOs.AlbumDtos;
using System.Security.Claims;

namespace MusicDistributionSystem.Controllers
{
    public class AlbumsController : Controller
    {
        private readonly IAlbumService _albumService;

        public AlbumsController(IAlbumService albumService) => _albumService = albumService;

        [HttpGet("/Albums")]
        public async Task<IActionResult> Index()
        {
            var albums = await _albumService.GetAllAlbumsAsync();
            return View(albums);
        }

        [HttpGet("/Albums/Details/{slug}")]
        public async Task<IActionResult> Details(string slug)
        {
            var album = await _albumService.GetAlbumBySlugAsync(slug);
            if (album is null) return NotFound();
            return View(album);
        }

        [HttpGet("/Albums/Create")]
        [Authorize(Policy = "CanUploadContent")]
        public IActionResult Create()
        {
            return View(new CreateAlbumRequestDto());
        }

        [HttpPost("/Albums/Create")]
        [Authorize(Policy = "CanUploadContent")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateAlbumRequestDto request)
        {
            if (!ModelState.IsValid) return View(request);

            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var result = await _albumService.CreateAlbumAsync(request, userId);
            if (!result.Succeeded)
            {
                ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "Failed to create album.");
                return View(request);
            }

            TempData["SuccessMessage"] = $"Album '{request.Title}' created successfully!";
            return RedirectToAction(nameof(Index));
        }
    }
}
