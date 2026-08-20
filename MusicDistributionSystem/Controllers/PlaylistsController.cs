using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MusicDistributionSystem.Application.Contracts.Services;
using MusicDistributionSystem.Application.DTOs.PlaylistDtos;
using System.Security.Claims;

namespace MusicDistributionSystem.Controllers
{
    public class PlaylistsController : Controller
    {
        private readonly IPlaylistService _playlistService;

        public PlaylistsController(IPlaylistService playlistService) => _playlistService = playlistService;

        [HttpGet("/Playlists")]
        public async Task<IActionResult> Index()
        {
            var publicLists = await _playlistService.GetPublicPlaylistsAsync();
            return View(publicLists);
        }

        [HttpGet("/Playlists/Details/{slug}")]
        public async Task<IActionResult> Details(string slug)
        {
            var playlist = await _playlistService.GetPlaylistBySlugAsync(slug);
            if (playlist is null) return NotFound();
            return View(playlist);
        }

        [HttpGet("/Playlists/My")]
        [Authorize]
        public async Task<IActionResult> MyPlaylists()
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var myLists = await _playlistService.GetUserPlaylistsAsync(userId);
            return View(myLists);
        }

        [HttpGet("/Playlists/Create")]
        [Authorize]
        public IActionResult Create()
        {
            return View(new CreatePlaylistRequestDto());
        }

        [HttpPost("/Playlists/Create")]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreatePlaylistRequestDto request)
        {
            if (!ModelState.IsValid) return View(request);

            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var result = await _playlistService.CreatePlaylistAsync(request, userId);
            if (!result.Succeeded)
            {
                ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "Failed to create playlist.");
                return View(request);
            }

            TempData["SuccessMessage"] = $"Playlist '{request.Title}' created!";
            return RedirectToAction(nameof(MyPlaylists));
        }

        [HttpPost("/Playlists/AddTrack")]
        [Authorize]
        public async Task<IActionResult> AddTrack(Guid playlistId, Guid trackId)
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var result = await _playlistService.AddTrackAsync(playlistId, trackId, userId);
            return Json(new { success = result.Succeeded, message = result.ErrorMessage ?? "Track added to playlist." });
        }

        [HttpPost("/Playlists/RemoveTrack")]
        [Authorize]
        public async Task<IActionResult> RemoveTrack(Guid playlistId, Guid trackId)
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var result = await _playlistService.RemoveTrackAsync(playlistId, trackId, userId);
            return Json(new { success = result.Succeeded });
        }
    }
}
