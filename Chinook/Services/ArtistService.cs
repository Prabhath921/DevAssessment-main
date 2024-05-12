using Chinook.ClientModels;
using Chinook.Models;
using Chinook.Pages;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;

namespace Chinook.Services
{
    public class ArtistService : IArtistService
    {
        IDbContextFactory<ChinookContext> DbFactory { get; set; }
        INavigationService NavigationService { get; set; }
        public ArtistService(IDbContextFactory<ChinookContext>  dbFactory, INavigationService navigationService)
        {
            DbFactory = dbFactory;
            NavigationService = navigationService;
        }
       
        public async Task<Artist> GetArtistByIdAsync(long artistId)
        {
            var DbContext = await DbFactory.CreateDbContextAsync();
            return DbContext.Artists.SingleOrDefault(a => a.ArtistId == artistId);
        }
        public async Task<List<PlaylistTrack>> GetTracksByArtistIdAsync(long artistId, string userId)
        {
            var DbContext = await DbFactory.CreateDbContextAsync();
            return DbContext.Tracks.Where(a => a.Album.ArtistId == artistId)
                .Include(a => a.Album)
                .Select(t => new PlaylistTrack()
                {
                    AlbumTitle = (t.Album == null ? "-" : t.Album.Title),
                    TrackId = t.TrackId,
                    TrackName = t.Name,
                    IsFavorite = t.Playlists.Where(p => p.UserPlaylists.Any(up => up.UserId == userId && up.Playlist.Name == "My favorite tracks")).Any()
                })
                .ToList();
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

        public async Task<List<Models.Playlist>> GetUserPlayListsAsync(string userId)
        {
            var DbContext = await DbFactory.CreateDbContextAsync();
            return DbContext.Playlists.Where(p => p.UserPlaylists.Any(up => up.UserId == userId && up.PlaylistId != 0)).ToList();
        }

        public async Task<bool> AddTrackToPlaylistAsync(long playlistId, string userId, string playlistName, long trackId)
        {
            var DbContext = await DbFactory.CreateDbContextAsync();
            bool isInPlayList = false;
            Models.Playlist? PlayList;
            if (playlistId == -1)
            {
                PlayList = DbContext.Playlists.Include(paylist => paylist.Tracks).Where(p => p.UserPlaylists.Any(up => up.UserId == userId && up.Playlist.Name == playlistName)).FirstOrDefault();
            }
            else
            {
                PlayList = DbContext.Playlists.Include(paylist => paylist.Tracks).Where(p => p.UserPlaylists.Any(up => up.UserId == userId && up.Playlist.PlaylistId == playlistId)).FirstOrDefault();
            }
            if (PlayList == null)
            {
                long SelectedPlayListId = (DbContext.Playlists?.Max(playlist => playlist.PlaylistId) ?? 0) + 1;
                await DbContext.UserPlaylists.AddAsync(
                    new UserPlaylist()
                    {
                        User = await DbContext.Users.SingleOrDefaultAsync(user => user.Id == userId),
                        Playlist = new Models.Playlist()
                        {
                            PlaylistId = SelectedPlayListId,
                            Name = playlistName,
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
                if (!PlayList.Tracks.Any(track => track.TrackId == trackId))
                {
                    PlayList.Tracks.Add(await DbContext.Tracks.SingleOrDefaultAsync(track => track.TrackId == trackId));
                }
                else
                    isInPlayList = true;
            }

            DbContext.SaveChanges();
            NavigationService.TriggerPlaylistChanged(userId);
            return isInPlayList;
        }
    }
}
