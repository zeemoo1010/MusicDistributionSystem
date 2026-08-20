using Microsoft.EntityFrameworkCore;
using MusicDistributionSystem.Domain.Contracts.Interface;
using MusicDistributionSystem.Domain.Entities;

namespace MusicDistributionSystem.Infrastructure.EntityFrameworkCore.Repositories
{
    public class PlaylistRepository : IPlaylistRepository
    {
        private readonly ApplicationDbContext _context;

        public PlaylistRepository(ApplicationDbContext context) => _context = context;

        public async Task<IReadOnlyCollection<Playlist>> GetPublicPlaylistsAsync()
        {
            return await _context.Playlists
                .AsNoTracking()
                .Include(p => p.User)
                .Include(p => p.PlaylistTracks)
                    .ThenInclude(pt => pt.MusicTrack)
                .Where(p => p.IsPublic)
                .OrderByDescending(p => p.IsEditorial)
                .ThenByDescending(p => p.CreatedAt)
                .ToListAsync();
        }

        public async Task<IReadOnlyCollection<Playlist>> GetUserPlaylistsAsync(Guid userId)
        {
            return await _context.Playlists
                .AsNoTracking()
                .Include(p => p.PlaylistTracks)
                    .ThenInclude(pt => pt.MusicTrack)
                .Where(p => p.UserId == userId)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
        }

        public Task<Playlist?> GetByIdWithTracksAsync(Guid id)
        {
            return _context.Playlists
                .Include(p => p.User)
                .Include(p => p.PlaylistTracks.OrderBy(pt => pt.OrderIndex))
                    .ThenInclude(pt => pt.MusicTrack)
                        .ThenInclude(t => t.Category)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public Task<Playlist?> GetBySlugWithTracksAsync(string slug)
        {
            return _context.Playlists
                .Include(p => p.User)
                .Include(p => p.PlaylistTracks.OrderBy(pt => pt.OrderIndex))
                    .ThenInclude(pt => pt.MusicTrack)
                        .ThenInclude(t => t.Category)
                .FirstOrDefaultAsync(p => p.Slug == slug);
        }

        public Task<Playlist?> GetByIdAsync(Guid id)
        {
            return _context.Playlists.FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task AddAsync(Playlist playlist) => await _context.Playlists.AddAsync(playlist);

        public async Task AddTrackToPlaylistAsync(PlaylistTrack playlistTrack)
        {
            var exists = await _context.PlaylistTracks.AnyAsync(pt =>
                pt.PlaylistId == playlistTrack.PlaylistId && pt.MusicTrackId == playlistTrack.MusicTrackId);

            if (!exists)
            {
                await _context.PlaylistTracks.AddAsync(playlistTrack);
            }
        }

        public async Task RemoveTrackFromPlaylistAsync(Guid playlistId, Guid trackId)
        {
            var item = await _context.PlaylistTracks.FirstOrDefaultAsync(pt =>
                pt.PlaylistId == playlistId && pt.MusicTrackId == trackId);

            if (item is not null)
            {
                _context.PlaylistTracks.Remove(item);
            }
        }

        public void Remove(Playlist playlist) => _context.Playlists.Remove(playlist);
        public Task SaveChangesAsync() => _context.SaveChangesAsync();
    }
}
