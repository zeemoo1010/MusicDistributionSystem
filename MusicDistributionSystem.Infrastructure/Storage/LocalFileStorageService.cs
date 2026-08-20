using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using MusicDistributionSystem.Application.Contracts.Infrastructure;

namespace MusicDistributionSystem.Infrastructure.Storage
{
    public class LocalFileStorageService : IFileStorageService
    {
        private readonly IWebHostEnvironment _environment;

        public LocalFileStorageService(IWebHostEnvironment environment)
        {
            _environment = environment;
        }

        public async Task<string> SaveFileAsync(IFormFile file, string subFolder, string sanitizedFileName, CancellationToken cancellationToken = default)
        {
            var folderPath = Path.Combine(_environment.WebRootPath, "uploads", subFolder);
            Directory.CreateDirectory(folderPath);

            var storedFileName = $"{Guid.NewGuid():N}-{sanitizedFileName}";
            var fullPath = Path.Combine(folderPath, storedFileName);

            await using (var stream = new FileStream(fullPath, FileMode.CreateNew, FileAccess.Write, FileShare.None))
            {
                await file.CopyToAsync(stream, cancellationToken);
            }

            var relativePath = Path.Combine("uploads", subFolder, storedFileName).Replace("\\", "/");
            return relativePath;
        }

        public Task DeleteFileAsync(string relativePath, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(relativePath))
            {
                return Task.CompletedTask;
            }

            var fullPath = GetPhysicalPath(relativePath);
            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
            }

            return Task.CompletedTask;
        }

        public bool FileExists(string relativePath)
        {
            if (string.IsNullOrWhiteSpace(relativePath))
            {
                return false;
            }

            var fullPath = GetPhysicalPath(relativePath);
            return File.Exists(fullPath);
        }

        public string GetPhysicalPath(string relativePath)
        {
            var uploadsRoot = Path.GetFullPath(Path.Combine(_environment.WebRootPath, "uploads"));
            var fullPath = Path.GetFullPath(Path.Combine(_environment.WebRootPath, relativePath.Replace("/", Path.DirectorySeparatorChar.ToString())));

            if (!fullPath.StartsWith(uploadsRoot, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("Invalid file path — attempted path traversal.");
            }

            return fullPath;
        }
    }
}
