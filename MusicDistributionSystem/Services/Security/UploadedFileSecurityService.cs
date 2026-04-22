using Microsoft.AspNetCore.Http;

namespace MusicDistributionSystem.Services.Security
{
    public class UploadedFileSecurityService : IUploadedFileSecurityService
    {
        private const long MaxMusicFileSizeBytes = 20 * 1024 * 1024;
        private static readonly string[] AllowedExtensions = [".mp3"];
        private static readonly string[] AllowedContentTypes = ["audio/mpeg", "audio/mp3", "application/octet-stream"];

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
    }
}
