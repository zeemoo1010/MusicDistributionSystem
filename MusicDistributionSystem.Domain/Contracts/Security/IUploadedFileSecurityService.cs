using Microsoft.AspNetCore.Http;

namespace MusicDistributionSystem.Domain.Contracts.Security
{
    public interface IUploadedFileSecurityService
    {
        Task<(bool IsValid, string? ErrorMessage)> ValidateMusicFileAsync(IFormFile file, CancellationToken cancellationToken = default);
        Task<(bool IsValid, string? ErrorMessage)> ValidateCoverImageAsync(IFormFile? file, CancellationToken cancellationToken = default);
        string SanitizeFileName(string fileName);
    }
}

