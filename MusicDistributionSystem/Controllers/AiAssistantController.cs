using Microsoft.AspNetCore.Mvc;
using MusicDistributionSystem.Application.Contracts.Services;

namespace MusicDistributionSystem.Controllers
{
    public class AiAssistantController : Controller
    {
        private readonly IAiContentService _aiService;

        public AiAssistantController(IAiContentService aiService) => _aiService = aiService;

        [HttpPost("/Ai/GenerateDescription")]
        public async Task<IActionResult> GenerateDescription([FromBody] GenerateDescRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Title) || string.IsNullOrWhiteSpace(request.Artist))
            {
                return BadRequest(new { message = "Title and Artist are required." });
            }

            var description = await _aiService.GenerateReleaseDescriptionAsync(
                request.Title, request.Artist, request.Category ?? "Afrobeats", request.Context);

            return Json(new { description });
        }

        [HttpPost("/Ai/GenerateTags")]
        public async Task<IActionResult> GenerateTags([FromBody] GenerateTagsRequest request)
        {
            var tags = await _aiService.GenerateTagsAsync(request.Title ?? "", request.Artist ?? "", request.Category ?? "Afrobeats");
            return Json(new { tags });
        }

        [HttpGet("/Ai/ParseSearch")]
        public async Task<IActionResult> ParseSearch(string query)
        {
            var (keywords, genre, mood) = await _aiService.ParseNaturalLanguageSearchAsync(query);
            return Json(new { keywords, genre, mood });
        }

        public class GenerateDescRequest
        {
            public string Title { get; set; } = string.Empty;
            public string Artist { get; set; } = string.Empty;
            public string? Category { get; set; }
            public string? Context { get; set; }
        }

        public class GenerateTagsRequest
        {
            public string? Title { get; set; }
            public string? Artist { get; set; }
            public string? Category { get; set; }
        }
    }
}
