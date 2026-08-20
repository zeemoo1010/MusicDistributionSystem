using MusicDistributionSystem.Application.DTOs.Account;
using MusicDistributionSystem.Application.DTOs.PlaylistDtos;

namespace MusicDistributionSystem.Application.Contracts.Services
{
    public interface IPlaylistService
    {
        Task<IReadOnlyCollection<PlaylistCardDto>> GetPublicPlaylistsAsync();
        Task<IReadOnlyCollection<PlaylistCardDto>> GetUserPlaylistsAsync(Guid userId);
        Task<PlaylistDetailsDto?> GetPlaylistBySlugAsync(string slug);
        Task<PlaylistDetailsDto?> GetPlaylistByIdAsync(Guid id);
        Task<OperationResultDto> CreatePlaylistAsync(CreatePlaylistRequestDto request, Guid userId);
        Task<OperationResultDto> AddTrackAsync(Guid playlistId, Guid trackId, Guid userId);
        Task<OperationResultDto> RemoveTrackAsync(Guid playlistId, Guid trackId, Guid userId);
        Task<OperationResultDto> DeletePlaylistAsync(Guid playlistId, Guid userId);
    }
}
