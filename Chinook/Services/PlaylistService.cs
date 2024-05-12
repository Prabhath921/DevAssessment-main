using Chinook.ClientModels;
using Chinook.Models;
using Microsoft.EntityFrameworkCore;

namespace Chinook.Services
{
    public class PlaylistService: IPlaylistService
    {
        IDbContextFactory<ChinookContext> DbFactory { get; set; }
        public PlaylistService(IDbContextFactory<ChinookContext> dbFactory)
        {
            DbFactory = dbFactory;
        }
        public async Task<ClientModels.Playlist> GetPlaylistByIdAsync(long playlistId, string userId)
        {
            var DbContext = await DbFactory.CreateDbContextAsync();

            return DbContext.Playlists
                .Include(a => a.Tracks).ThenInclude(a => a.Album).ThenInclude(a => a.Artist)
                .Where(p => p.PlaylistId == playlistId)
                .Select(p => new ClientModels.Playlist()
                {
                    Name = p.Name,
                    Tracks = p.Tracks.Select(t => new ClientModels.PlaylistTrack()
                    {
                        AlbumTitle = t.Album.Title,
                        ArtistName = t.Album.Artist.Name,
                        TrackId = t.TrackId,
                        TrackName = t.Name,
                        IsFavorite = t.Playlists.Where(p => p.UserPlaylists.Any(up => up.UserId == userId)).Any()
                    }).ToList()
                })
                .FirstOrDefault();
        }

        public async Task AddFavoriteTrackAsync(long trackId, string userId)
        {
            var DbContext = await DbFactory.CreateDbContextAsync();

            Models.Playlist? FavoritePlayList = DbContext.Playlists.Where(p => p.UserPlaylists.Any(up => up.UserId == userId && up.Playlist.Name == "My favorite tracks")).FirstOrDefault();
            if (FavoritePlayList == null)
            {
                await DbContext.UserPlaylists.AddAsync(
                    new UserPlaylist()
                    {
                        User = await DbContext.Users.SingleOrDefaultAsync(user => user.Id == userId),
                        Playlist = new Models.Playlist()
                        {
                            PlaylistId = 0,
                            Name = "My favorite tracks",
                            Tracks = new List<Track>()
                                {
                                    await DbContext.Tracks.SingleOrDefaultAsync(track => track.TrackId == trackId)
                                }
                        }
                    }
                );
            }
            else
            {
                FavoritePlayList.Tracks.Add(await DbContext.Tracks.SingleOrDefaultAsync(track => track.TrackId == trackId));
            }

            DbContext.SaveChanges();
        }

        public async Task RemoveFavoriteTrackAsync(long trackId, string userId)
        {
            var DbContext = await DbFactory.CreateDbContextAsync();
            Models.Playlist? FavoritePlayList = DbContext.Playlists.Include(paylist => paylist.Tracks).Where(p => p.UserPlaylists.Any(up => up.UserId == userId && up.Playlist.Name == "My favorite tracks")).FirstOrDefault();
            FavoritePlayList.Tracks.Remove(await DbContext.Tracks.SingleOrDefaultAsync(track => track.TrackId == trackId));
            DbContext.SaveChanges();
        }

        public async Task RemoveTrackByIdAsync(long trackId, long playlistId)
        {
            var DbContext = await DbFactory.CreateDbContextAsync();
            var track = DbContext.Tracks.FirstOrDefault(t => t.TrackId == trackId);
            DbContext.Playlists.FirstOrDefault(playlist => playlist.PlaylistId == playlistId).Tracks.Remove(track);
            DbContext.SaveChanges();
        }
    }
}
