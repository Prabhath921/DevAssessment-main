using Chinook.ClientModels;
using Chinook.Models;

namespace Chinook.Services
{
    public interface IArtistService
    {
        Task<Artist> GetArtistByIdAsync(long artistId);
        Task<List<PlaylistTrack>> GetTracksByArtistIdAsync(long artistId, string userId);
        Task AddFavoriteTrackAsync(long trackId, string userId);
        Task RemoveFavoriteTrackAsync(long trackId, string userId);
        Task<List<Models.Playlist>> GetUserPlayListsAsync(string userId);
        Task<bool> AddTrackToPlaylistAsync(long playlistId, string userId, string playlistName, long trackId);
    }
}
