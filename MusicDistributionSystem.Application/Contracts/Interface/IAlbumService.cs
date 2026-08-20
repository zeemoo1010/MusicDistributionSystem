using MusicDistributionSystem.Application.DTOs.Account;
using MusicDistributionSystem.Application.DTOs.AlbumDtos;

namespace MusicDistributionSystem.Application.Contracts.Services
{
    public interface IAlbumService
    {
        Task<IReadOnlyCollection<AlbumCardDto>> GetFeaturedAlbumsAsync(int take = 6);
        Task<IReadOnlyCollection<AlbumCardDto>> GetAllAlbumsAsync();
        Task<AlbumDetailsDto?> GetAlbumBySlugAsync(string slug);
        Task<AlbumDetailsDto?> GetAlbumByIdAsync(Guid id);
        Task<OperationResultDto> CreateAlbumAsync(CreateAlbumRequestDto request, Guid uploadedByUserId);
    }
}
