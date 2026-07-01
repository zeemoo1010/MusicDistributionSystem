using Microsoft.AspNetCore.Http;
using MusicDistributionSystem.Domain.Contracts.Security;

namespace MusicDistributionSystem.Infrastructure.Security
{
    public class UploadedFileSecurityService : IUploadedFileSecurityService
    {
        private const long MaxMusicFileSizeBytes = 20 * 1024 * 1024;
        private const long MaxCoverImageSizeBytes = 5 * 1024 * 1024;
        private static readonly string[] AllowedExtensions = [".mp3"];
        private static readonly string[] AllowedContentTypes = ["audio/mpeg", "audio/mp3"];
        private static readonly string[] AllowedImageExtensions = [".jpg", ".jpeg", ".png"];
        private static readonly string[] AllowedImageContentTypes = ["image/jpeg", "image/png"];

        public async Task<(bool IsValid, string? ErrorMessage)> ValidateMusicFileAsync(IFormFile file, CancellationToken cancellationToken = default)
        {
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!AllowedExtensions.Contains(extension))
            {
                return (false, "Only MP3 files are allowed.");
            }

            if (file.Length == 0 || file.Length > MaxMusicFileSizeBytes)
            {
                return (false, "The file must be between 1 byte and 20 MB.");
            }

            if (!AllowedContentTypes.Contains(file.ContentType))
            {
                return (false, "The uploaded file content type is not allowed.");
            }

            await using var stream = file.OpenReadStream();
            var header = new byte[3];
            var bytesRead = await stream.ReadAsync(header.AsMemory(0, header.Length), cancellationToken);

            if (bytesRead < 2)
            {
                return (false, "The uploaded MP3 file is invalid or empty.");
            }

            var hasId3Header = bytesRead >= 3 && header[0] == 0x49 && header[1] == 0x44 && header[2] == 0x33;
            var hasMp3FrameSync = header[0] == 0xFF && (header[1] & 0xE0) == 0xE0;

            if (!hasId3Header && !hasMp3FrameSync)
            {
                return (false, "The uploaded file does not look like a valid MP3.");
            }

            return (true, null);
        }

        public Task<(bool IsValid, string? ErrorMessage)> ValidateCoverImageAsync(IFormFile? file, CancellationToken cancellationToken = default)
        {
            if (file is null)
            {
                return Task.FromResult<(bool IsValid, string? ErrorMessage)>((true, null));
            }

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!AllowedImageExtensions.Contains(extension))
            {
                return Task.FromResult<(bool IsValid, string? ErrorMessage)>((false, "Only JPG and PNG cover images are allowed."));
            }

            if (file.Length == 0 || file.Length > MaxCoverImageSizeBytes)
            {
                return Task.FromResult<(bool IsValid, string? ErrorMessage)>((false, "Cover images must be between 1 byte and 5 MB."));
            }

            if (!AllowedImageContentTypes.Contains(file.ContentType))
            {
                return Task.FromResult<(bool IsValid, string? ErrorMessage)>((false, "The uploaded cover image content type is not allowed."));
            }

            return Task.FromResult<(bool IsValid, string? ErrorMessage)>((true, null));
        }

        public string SanitizeFileName(string fileName)
        {
            var baseName = Path.GetFileNameWithoutExtension(fileName);
            var extension = Path.GetExtension(fileName).ToLowerInvariant();

            // Remove any characters that are invalid in Windows/Linux filenames
            var invalidChars = Path.GetInvalidFileNameChars();
            var sanitized = string.Concat(baseName
                .Select(c => invalidChars.Contains(c) ? '-' : c))
                .TrimEnd('.');

            // Replace sequences of dashes/underscores with single dash
            var normalized = System.Text.RegularExpressions.Regex.Replace(sanitized, @"[-_]{2,}", "-").Trim('-');

            if (string.IsNullOrWhiteSpace(normalized))
            {
                normalized = "file";
            }

            // Ensure the full filename doesn't attempt path traversal
            var fullName = $"{normalized}{extension}";
            if (fullName.Contains("..") || fullName.Contains('/') || fullName.Contains('\\'))
            {
                fullName = $"file{extension}";
            }

            return fullName;
        }
    }
}