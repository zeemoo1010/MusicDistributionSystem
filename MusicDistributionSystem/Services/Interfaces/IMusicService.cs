using MusicDistributionSystem.DTOs.Music;

namespace MusicDistributionSystem.Services.Interfaces
{
    public interface IMusicService
    {
        Task<MusicIndexDto> GetMusicIndexAsync(string? searchTerm, Guid? categoryId);
        Task<MusicDetailsDto?> GetMusicDetailsAsync(Guid id);
        Task<MusicUploadRequestDto> GetUploadFormAsync();
        Task<MusicUploadResultDto> UploadAsync(MusicUploadRequestDto request, Guid uploaderUserId, string uploaderName, string uploaderEmail);
        Task<MusicDownloadResultDto> PrepareDownloadAsync(Guid id, string? downloaderIpAddress);
    }
}
