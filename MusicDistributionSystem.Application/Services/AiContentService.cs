using MusicDistributionSystem.Application.Contracts.Services;
using System.Text.RegularExpressions;

namespace MusicDistributionSystem.Application.Services
{
    public class AiContentService : IAiContentService
    {
        public Task<string> GenerateReleaseDescriptionAsync(string title, string artist, string category, string? additionalContext = null)
        {
            var cleanTitle = title.Trim();
            var cleanArtist = artist.Trim();
            var cleanCategory = category.Trim();

            var templates = new[]
            {
                $"{cleanArtist} delivers an electrifying new release titled \"{cleanTitle}\", immersing listeners in signature {cleanCategory} vibrations. Featuring captivating rhythms and polished production, this record stands out as a vibrant addition to the current soundscape.",
                $"\"{cleanTitle}\" is the latest standout single from {cleanArtist}. Blending rich {cleanCategory} grooves with infectious melodies, this release captures authentic artistry and undeniable replay value.",
                $"Acclaimed talent {cleanArtist} drops \"{cleanTitle}\", a masterclass in modern {cleanCategory}. With crisp arrangement and soulful energy, this anthem is tailored for high-rotation playlists and sound systems worldwide."
            };

            var selected = templates[Math.Abs(cleanTitle.GetHashCode()) % templates.Length];
            if (!string.IsNullOrWhiteSpace(additionalContext))
            {
                selected += $" {additionalContext.Trim()}";
            }

            return Task.FromResult(selected);
        }

        public Task<IReadOnlyCollection<string>> GenerateTagsAsync(string title, string artist, string category)
        {
            var tags = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                category,
                "Trending",
                "NewMusic",
                "Afrobeats",
                "HitSingle",
                "SoundSphereExclusive"
            };

            if (!string.IsNullOrWhiteSpace(artist))
            {
                tags.Add(artist.Replace(" ", ""));
            }

            return Task.FromResult<IReadOnlyCollection<string>>(tags.ToList());
        }

        public Task<(string SearchKeywords, string? Genre, string? Mood)> ParseNaturalLanguageSearchAsync(string prompt)
        {
            if (string.IsNullOrWhiteSpace(prompt))
            {
                return Task.FromResult<(string, string?, string?)>((string.Empty, null, null));
            }

            var lower = prompt.ToLowerInvariant();
            string? detectedGenre = null;
            string? detectedMood = null;

            if (lower.Contains("afrobeats") || lower.Contains("afro")) detectedGenre = "Afrobeats";
            else if (lower.Contains("gospel") || lower.Contains("worship") || lower.Contains("praise")) detectedGenre = "Gospel";
            else if (lower.Contains("hip hop") || lower.Contains("rap") || lower.Contains("trap")) detectedGenre = "Hip Hop";
            else if (lower.Contains("highlife")) detectedGenre = "Highlife";
            else if (lower.Contains("mixtape") || lower.Contains("dj mix")) detectedGenre = "Mixtapes";

            if (lower.Contains("party") || lower.Contains("dance") || lower.Contains("club") || lower.Contains("upbeat")) detectedMood = "Party & Energetic";
            else if (lower.Contains("chill") || lower.Contains("relax") || lower.Contains("slow") || lower.Contains("smooth")) detectedMood = "Chill & Relaxed";
            else if (lower.Contains("love") || lower.Contains("romantic")) detectedMood = "Romantic";
            else if (lower.Contains("sad") || lower.Contains("emotional")) detectedMood = "Emotional";

            // Strip filler words to leave keywords
            var cleanKeywords = Regex.Replace(lower, @"\b(play|find|search|give|me|some|good|latest|new|best|songs|tracks|music|mp3|download|video)\b", "", RegexOptions.IgnoreCase).Trim();
            cleanKeywords = Regex.Replace(cleanKeywords, @"\s+", " ");

            return Task.FromResult((cleanKeywords, detectedGenre, detectedMood));
        }
    }
}
