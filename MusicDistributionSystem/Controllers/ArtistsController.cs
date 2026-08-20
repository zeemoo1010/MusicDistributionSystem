using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MusicDistributionSystem.Application.Contracts.Services;
using MusicDistributionSystem.Application.DTOs.ArtistDtos;

namespace MusicDistributionSystem.Controllers
{
    public class ArtistsController : Controller
    {
        private readonly IArtistService _artistService;

        public ArtistsController(IArtistService artistService) => _artistService = artistService;

        [HttpGet("/Artists")]
        public async Task<IActionResult> Index()
        {
            var artists = await _artistService.GetAllArtistsAsync();
            return View(artists);
        }

        [HttpGet("/Artists/Details/{slug}")]
        public async Task<IActionResult> Details(string slug)
        {
            var artist = await _artistService.GetArtistBySlugAsync(slug);
            if (artist is null) return NotFound();
            return View(artist);
        }

        [HttpGet("/Artists/Create")]
        [Authorize(Policy = "CanUploadContent")]
        public IActionResult Create()
        {
            return View(new CreateArtistRequestDto());
        }

        [HttpPost("/Artists/Create")]
        [Authorize(Policy = "CanUploadContent")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateArtistRequestDto request)
        {
            if (!ModelState.IsValid) return View(request);

            var result = await _artistService.CreateArtistAsync(request);
            if (!result.Succeeded)
            {
                ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "Failed to create artist.");
                return View(request);
            }

            TempData["SuccessMessage"] = $"Artist '{request.Name}' created successfully!";
            return RedirectToAction(nameof(Index));
        }
    }
}
