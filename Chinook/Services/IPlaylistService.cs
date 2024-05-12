namespace Chinook.Services
{
    public interface IPlaylistService
    {
        Task<ClientModels.Playlist> GetPlaylistByIdAsync(long playlistId, string userId);
        Task AddFavoriteTrackAsync(long trackId, string userId);
        Task RemoveFavoriteTrackAsync(long trackId, string userId);
        Task RemoveTrackByIdAsync(long trackId, long playlistId);
    }
}
