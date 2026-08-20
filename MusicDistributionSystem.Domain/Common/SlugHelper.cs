using System.Text.RegularExpressions;

namespace MusicDistributionSystem.Domain.Common
{
    public static class SlugHelper
    {
        public static string GenerateSlug(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return Guid.NewGuid().ToString("N")[..8];
            }

            var str = text.ToLowerInvariant().Trim();

            // Replace invalid characters with space
            str = Regex.Replace(str, @"[^a-z0-9\s-]", "");

            // Convert multiple spaces into a single dash
            str = Regex.Replace(str, @"\s+", "-").Trim('-');

            // Cut off at max 100 chars
            if (str.Length > 100)
            {
                str = str[..100].TrimEnd('-');
            }

            return string.IsNullOrWhiteSpace(str) ? Guid.NewGuid().ToString("N")[..8] : str;
        }
    }
}
