using MusicDistributionSystem.Application.DTOs.Account;
using MusicDistributionSystem.Application.DTOs.Music;
using MusicDistributionSystem.Application.DTOs.VideoDtos;

namespace MusicDistributionSystem.Application.Contracts.Services
{
    public interface IVideoService
    {
        Task<VideoIndexDto> GetVideoIndexAsync(string? searchTerm, Guid? categoryId, int page = 1, int pageSize = 12);
        Task<IReadOnlyCollection<VideoCardDto>> GetFeaturedVideosAsync(int take = 6);
        Task<VideoDetailsDto?> GetVideoDetailsAsync(Guid id);
        Task<VideoDetailsDto?> GetVideoDetailsBySlugAsync(string slug);
        Task<VideoUploadRequestDto> GetUploadFormAsync();
        Task<OperationResultDto> UploadAsync(VideoUploadRequestDto request, Guid uploaderUserId, string uploaderName, string uploaderEmail);
        Task<MusicDownloadResultDto> PrepareDownloadAsync(Guid id, Guid? userId, string? downloaderIpAddress);
        Task<OperationResultDto> AddCommentAsync(Guid videoId, Guid userId, string content);
        Task<OperationResultDto> ToggleLikeAsync(Guid videoId, Guid userId);
    }
}
