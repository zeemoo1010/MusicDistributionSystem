using MusicDistributionSystem.Application.DTOs.Account;
using MusicDistributionSystem.Application.DTOs.ArtistDtos;

namespace MusicDistributionSystem.Application.Contracts.Services
{
    public interface IArtistService
    {
        Task<IReadOnlyCollection<ArtistCardDto>> GetFeaturedArtistsAsync(int take = 8);
        Task<IReadOnlyCollection<ArtistCardDto>> GetAllArtistsAsync();
        Task<ArtistDetailsDto?> GetArtistBySlugAsync(string slug);
        Task<ArtistDetailsDto?> GetArtistByIdAsync(Guid id);
        Task<OperationResultDto> CreateArtistAsync(CreateArtistRequestDto request);
    }
}
