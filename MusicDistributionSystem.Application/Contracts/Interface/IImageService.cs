using MusicDistributionSystem.Application.DTOs.Account;
using MusicDistributionSystem.Application.DTOs.ImageDtos;
using MusicDistributionSystem.Application.DTOs.Music;

namespace MusicDistributionSystem.Application.Contracts.Services
{
    public interface IImageService
    {
        Task<ImageIndexDto> GetImageIndexAsync(string? searchTerm, Guid? categoryId, int page = 1, int pageSize = 16);
        Task<IReadOnlyCollection<ImageCardDto>> GetFeaturedImagesAsync(int take = 8);
        Task<ImageDetailsDto?> GetImageDetailsAsync(Guid id);
        Task<ImageDetailsDto?> GetImageDetailsBySlugAsync(string slug);
        Task<ImageUploadRequestDto> GetUploadFormAsync();
        Task<OperationResultDto> UploadAsync(ImageUploadRequestDto request, Guid uploaderUserId, string uploaderName, string uploaderEmail);
        Task<MusicDownloadResultDto> PrepareDownloadAsync(Guid id, Guid? userId, string? downloaderIpAddress);
    }
}
