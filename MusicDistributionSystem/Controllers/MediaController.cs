using Microsoft.AspNetCore.Mvc;
using MusicDistributionSystem.Application.Contracts.Services;
using System.Security.Claims;

namespace MusicDistributionSystem.Controllers
{
    public class MediaController : Controller
    {
        private readonly IMediaStreamingService _streamingService;
        private readonly IMusicService _musicService;
        private readonly IVideoService _videoService;
        private readonly IImageService _imageService;

        public MediaController(
            IMediaStreamingService streamingService,
            IMusicService musicService,
            IVideoService videoService,
            IImageService imageService)
        {
            _streamingService = streamingService;
            _musicService = musicService;
            _videoService = videoService;
            _imageService = imageService;
        }

        [HttpGet("/Media/StreamAudio/{id:guid}")]
        public async Task<IActionResult> StreamAudio(Guid id)
        {
            var userId = GetCurrentUserId();
            var (found, allowed, filePath, contentType, fileLength, errorMessage) =
                await _streamingService.GetMediaStreamAsync(id, "music", userId);

            if (!found) return NotFound(new { message = errorMessage });
            if (!allowed) return StatusCode(403, new { message = errorMessage });

            // Record play count asynchronously
            var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
            _ = _streamingService.RecordPlayAsync(id, userId, ip);

            // Return physical file with enableRangeProcessing: true for HTTP 206 Byte-Range streaming
            return PhysicalFile(filePath!, contentType ?? "audio/mpeg", enableRangeProcessing: true);
        }

        [HttpGet("/Media/StreamVideo/{id:guid}")]
        public async Task<IActionResult> StreamVideo(Guid id)
        {
            var userId = GetCurrentUserId();
            var (found, allowed, filePath, contentType, fileLength, errorMessage) =
                await _streamingService.GetMediaStreamAsync(id, "video", userId);

            if (!found) return NotFound(new { message = errorMessage });
            if (!allowed) return StatusCode(403, new { message = errorMessage });

            // Record video view
            var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
            _ = _streamingService.RecordVideoViewAsync(id, userId, ip);

            return PhysicalFile(filePath!, contentType ?? "video/mp4", enableRangeProcessing: true);
        }

        [HttpGet("/Media/DownloadMusic/{id:guid}")]
        public async Task<IActionResult> DownloadMusic(Guid id)
        {
            var userId = GetCurrentUserId();
            var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
            var result = await _musicService.PrepareDownloadAsync(id, userId, ip);

            if (!result.Found) return NotFound("Track not found.");
            if (!result.Allowed)
            {
                TempData["ErrorMessage"] = result.ErrorMessage;
                return RedirectToAction("Plans", "Payment");
            }
            if (!result.FileExists) return NotFound("File is missing on server.");

            return PhysicalFile(result.FilePath, result.ContentType, result.OriginalFileName);
        }

        [HttpGet("/Media/DownloadVideo/{id:guid}")]
        public async Task<IActionResult> DownloadVideo(Guid id)
        {
            var userId = GetCurrentUserId();
            var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
            var result = await _videoService.PrepareDownloadAsync(id, userId, ip);

            if (!result.Found) return NotFound("Video not found.");
            if (!result.Allowed)
            {
                TempData["ErrorMessage"] = result.ErrorMessage;
                return RedirectToAction("Plans", "Payment");
            }
            if (!result.FileExists) return NotFound("File is missing on server.");

            return PhysicalFile(result.FilePath, result.ContentType, result.OriginalFileName);
        }

        [HttpGet("/Media/DownloadImage/{id:guid}")]
        public async Task<IActionResult> DownloadImage(Guid id)
        {
            var userId = GetCurrentUserId();
            var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
            var result = await _imageService.PrepareDownloadAsync(id, userId, ip);

            if (!result.Found) return NotFound("Image not found.");
            if (!result.Allowed)
            {
                TempData["ErrorMessage"] = result.ErrorMessage;
                return RedirectToAction("Plans", "Payment");
            }
            if (!result.FileExists) return NotFound("File is missing on server.");

            return PhysicalFile(result.FilePath, result.ContentType, result.OriginalFileName);
        }

        private Guid? GetCurrentUserId()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (claim is not null && Guid.TryParse(claim.Value, out var guid))
            {
                return guid;
            }
            return null;
        }
    }
}
