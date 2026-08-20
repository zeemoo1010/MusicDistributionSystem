using MusicDistributionSystem.Application.DTOs.Account;
using MusicDistributionSystem.Application.DTOs.Music;

namespace MusicDistributionSystem.Application.Contracts.Services
{
    public interface IMusicService
    {
        Task<MusicIndexDto> GetMusicIndexAsync(string? searchTerm, Guid? categoryId, int page = 1, int pageSize = 12);
        Task<MusicDetailsDto?> GetMusicDetailsAsync(Guid id);
        Task<MusicDetailsDto?> GetMusicDetailsBySlugAsync(string slug);
        Task<IReadOnlyCollection<MusicCardDto>> GetTrendingTracksAsync(int take = 10);
        Task<IReadOnlyCollection<MusicCardDto>> GetTopChartTracksAsync(int take = 10);
        Task<MusicUploadRequestDto> GetUploadFormAsync();
        Task<MusicUploadResultDto> UploadAsync(MusicUploadRequestDto request, Guid uploaderUserId, string uploaderName, string uploaderEmail);
        Task<MusicDownloadResultDto> PrepareDownloadAsync(Guid id, Guid? userId, string? downloaderIpAddress);
        Task<OperationResultDto> AddCommentAsync(Guid trackId, Guid userId, string content);
        Task<OperationResultDto> ToggleLikeAsync(Guid trackId, Guid userId);
    }
}


