namespace MusicDistributionSystem.Application.Contracts.Services
{
    public interface IAiContentService
    {
        Task<string> GenerateReleaseDescriptionAsync(string title, string artist, string category, string? additionalContext = null);
        Task<IReadOnlyCollection<string>> GenerateTagsAsync(string title, string artist, string category);
        Task<(string SearchKeywords, string? Genre, string? Mood)> ParseNaturalLanguageSearchAsync(string prompt);
    }
}
