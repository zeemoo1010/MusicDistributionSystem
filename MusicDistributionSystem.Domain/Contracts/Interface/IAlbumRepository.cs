using MusicDistributionSystem.Domain.Common;
using MusicDistributionSystem.Domain.Entities;

namespace MusicDistributionSystem.Domain.Contracts.Interface
{
    public interface IAlbumRepository
    {
        Task<IReadOnlyCollection<Album>> GetAllAsync();
        Task<PaginatedResult<Album>> GetPagedAsync(string? searchTerm, Guid? categoryId, Guid? artistId, int page, int pageSize);
        Task<Album?> GetByIdAsync(Guid id, bool asNoTracking = true);
        Task<Album?> GetBySlugAsync(string slug, bool asNoTracking = true);
        Task<IReadOnlyCollection<Album>> GetFeaturedAlbumsAsync(int take);
        Task AddAsync(Album album);
        void Remove(Album album);
        Task SaveChangesAsync();
    }
}
