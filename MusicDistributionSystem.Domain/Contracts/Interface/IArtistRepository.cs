using MusicDistributionSystem.Domain.Common;
using MusicDistributionSystem.Domain.Entities;

namespace MusicDistributionSystem.Domain.Contracts.Interface
{
    public interface IArtistRepository
    {
        Task<IReadOnlyCollection<Artist>> GetAllAsync();
        Task<PaginatedResult<Artist>> GetPagedAsync(string? searchTerm, int page, int pageSize);
        Task<Artist?> GetByIdAsync(Guid id, bool asNoTracking = true);
        Task<Artist?> GetBySlugAsync(string slug, bool asNoTracking = true);
        Task<IReadOnlyCollection<Artist>> GetFeaturedArtistsAsync(int take);
        Task AddAsync(Artist artist);
        void Remove(Artist artist);
        Task SaveChangesAsync();
    }
}
