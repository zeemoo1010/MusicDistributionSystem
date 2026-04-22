using Microsoft.AspNetCore.Http;

namespace MusicDistributionSystem.Services.Security
{
    public interface IUploadedFileSecurityService
    {
        Task<(bool IsValid, string? ErrorMessage)> ValidateMusicFileAsync(IFormFile file, CancellationToken cancellationToken = default);
    }
}
