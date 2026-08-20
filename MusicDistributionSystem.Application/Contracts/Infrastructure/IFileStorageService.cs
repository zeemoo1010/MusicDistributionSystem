using Microsoft.AspNetCore.Http;

namespace MusicDistributionSystem.Application.Contracts.Infrastructure
{
    public interface IFileStorageService
    {
        Task<string> SaveFileAsync(IFormFile file, string subFolder, string sanitizedFileName, CancellationToken cancellationToken = default);
        Task DeleteFileAsync(string relativePath, CancellationToken cancellationToken = default);
        bool FileExists(string relativePath);
        string GetPhysicalPath(string relativePath);
    }
}
