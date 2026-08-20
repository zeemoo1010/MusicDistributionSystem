using MusicDistributionSystem.Domain.Entities;

namespace MusicDistributionSystem.Domain.Contracts.Interface
{
    public interface IPlaylistRepository
    {
        Task<IReadOnlyCollection<Playlist>> GetPublicPlaylistsAsync();
        Task<IReadOnlyCollection<Playlist>> GetUserPlaylistsAsync(Guid userId);
        Task<Playlist?> GetByIdWithTracksAsync(Guid id);
        Task<Playlist?> GetBySlugWithTracksAsync(string slug);
        Task<Playlist?> GetByIdAsync(Guid id);
        Task AddAsync(Playlist playlist);
        Task AddTrackToPlaylistAsync(PlaylistTrack playlistTrack);
        Task RemoveTrackFromPlaylistAsync(Guid playlistId, Guid trackId);
        void Remove(Playlist playlist);
        Task SaveChangesAsync();
    }
}
